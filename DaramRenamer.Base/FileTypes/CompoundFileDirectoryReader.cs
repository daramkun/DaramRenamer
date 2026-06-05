using System.Text;

namespace DaramRenamer.FileTypes;

internal static class CompoundFileDirectoryReader
{
    private const int EndOfChain = unchecked((int)0xFFFFFFFE);
    private const int FreeSector = unchecked((int)0xFFFFFFFF);
    private const int FatSector = unchecked((int)0xFFFFFFFD);
    private const int DifatSector = unchecked((int)0xFFFFFFFC);

    public static HashSet<string> ReadDirectoryNames(Stream stream)
    {
        try
        {
            var file = Read(stream);
            return file.Entries.Select(entry => entry.Name).Where(name => name.Length > 0).ToHashSet(StringComparer.Ordinal);
        }
        catch
        {
            return [];
        }
    }

    public static bool TryReadStream(Stream stream, string name, out byte[] bytes)
    {
        bytes = [];
        try
        {
            var file = Read(stream);
            var entry = file.Entries.FirstOrDefault(entry => entry.Name == name);
            if (entry == null || entry.StartSector < 0 || entry.Size <= 0)
                return false;

            bytes = entry.Size < file.MiniStreamCutoffSize && file.MiniFat.Length > 0
                ? ReadMiniStream(file, entry)
                : ReadFatChain(file, entry.StartSector, (int)Math.Min(entry.Size, int.MaxValue));
            return bytes.Length > 0;
        }
        catch
        {
            bytes = [];
            return false;
        }
    }

    private static CompoundFile Read(Stream stream)
    {
        var origin = stream.Position;
        try
        {
            stream.Position = 0;
            using var reader = new BinaryReader(stream, Encoding.Unicode, true);
            var header = reader.ReadBytes(512);
            if (header.Length < 512)
                throw new InvalidDataException();

            var sectorShift = ReadUInt16(header, 30);
            var miniSectorShift = ReadUInt16(header, 32);
            var sectorSize = 1 << sectorShift;
            var miniSectorSize = 1 << miniSectorShift;
            var fatSectorCount = ReadInt32(header, 44);
            var firstDirectorySector = ReadInt32(header, 48);
            var miniStreamCutoffSize = ReadInt32(header, 56);
            var firstMiniFatSector = ReadInt32(header, 60);
            var miniFatSectorCount = ReadInt32(header, 64);
            var firstDifatSector = ReadInt32(header, 68);
            var difatSectorCount = ReadInt32(header, 72);

            var difat = new List<int>(fatSectorCount);
            for (var offset = 76; offset < 512 && difat.Count < fatSectorCount; offset += 4)
                AddSectorIndex(difat, ReadInt32(header, offset));

            var nextDifat = firstDifatSector;
            for (var i = 0; i < difatSectorCount && nextDifat >= 0; ++i)
            {
                var sector = ReadSector(stream, sectorSize, nextDifat);
                for (var offset = 0; offset < sectorSize - 4 && difat.Count < fatSectorCount; offset += 4)
                    AddSectorIndex(difat, ReadInt32(sector, offset));
                nextDifat = ReadInt32(sector, sectorSize - 4);
            }

            var fat = new List<int>();
            foreach (var sectorIndex in difat)
            {
                var sector = ReadSector(stream, sectorSize, sectorIndex);
                for (var offset = 0; offset < sectorSize; offset += 4)
                    fat.Add(ReadInt32(sector, offset));
            }

            var directoryBytes = ReadFatChain(stream, sectorSize, fat, firstDirectorySector, int.MaxValue);
            var entries = ReadDirectoryEntries(directoryBytes);
            var root = entries.FirstOrDefault(entry => entry.ObjectType == 5);
            var miniStream = root is { StartSector: >= 0, Size: > 0 }
                ? ReadFatChain(stream, sectorSize, fat, root.StartSector, (int)Math.Min(root.Size, int.MaxValue))
                : [];

            var miniFat = firstMiniFatSector >= 0 && miniFatSectorCount > 0
                ? ReadFatChain(stream, sectorSize, fat, firstMiniFatSector, miniFatSectorCount * sectorSize)
                    .Chunk(4)
                    .Where(chunk => chunk.Length == 4)
                    .Select(chunk => BitConverter.ToInt32(chunk))
                    .ToArray()
                : [];

            return new CompoundFile(stream, sectorSize, miniSectorSize, miniStreamCutoffSize, fat.ToArray(), miniFat, miniStream, entries);
        }
        finally
        {
            stream.Position = origin;
        }
    }

    private static byte[] ReadFatChain(CompoundFile file, int startSector, int maxBytes) =>
        ReadFatChain(file.Stream, file.SectorSize, file.Fat, startSector, maxBytes);

    private static byte[] ReadFatChain(Stream stream, int sectorSize, IReadOnlyList<int> fat, int startSector, int maxBytes)
    {
        using var output = new MemoryStream();
        var sector = startSector;
        var guard = 0;
        while (sector >= 0 && sector < fat.Count && sector is not EndOfChain and not FreeSector and not FatSector and not DifatSector && guard++ < fat.Count)
        {
            var bytes = ReadSector(stream, sectorSize, sector);
            var remaining = maxBytes == int.MaxValue ? bytes.Length : Math.Min(bytes.Length, maxBytes - (int)output.Length);
            if (remaining <= 0)
                break;
            output.Write(bytes, 0, remaining);
            if (output.Length >= maxBytes)
                break;
            sector = fat[sector];
        }
        return output.ToArray();
    }

    private static byte[] ReadMiniStream(CompoundFile file, DirectoryEntry entry)
    {
        using var output = new MemoryStream();
        var sector = entry.StartSector;
        var guard = 0;
        while (sector >= 0 && sector < file.MiniFat.Length && sector != EndOfChain && guard++ < file.MiniFat.Length)
        {
            var offset = sector * file.MiniSectorSize;
            if (offset < 0 || offset >= file.MiniStream.Length)
                break;
            var remainingInSector = Math.Min(file.MiniSectorSize, file.MiniStream.Length - offset);
            var remaining = Math.Min(remainingInSector, (int)Math.Min(entry.Size - output.Length, int.MaxValue));
            if (remaining <= 0)
                break;
            output.Write(file.MiniStream, offset, remaining);
            sector = file.MiniFat[sector];
        }
        return output.ToArray();
    }

    private static List<DirectoryEntry> ReadDirectoryEntries(byte[] directoryBytes)
    {
        var entries = new List<DirectoryEntry>();
        for (var offset = 0; offset + 128 <= directoryBytes.Length; offset += 128)
        {
            var nameLength = ReadUInt16(directoryBytes, offset + 64);
            var nameBytes = Math.Clamp(nameLength - 2, 0, 64);
            var name = nameBytes > 0 ? Encoding.Unicode.GetString(directoryBytes, offset, nameBytes) : string.Empty;
            entries.Add(new DirectoryEntry(
                name,
                directoryBytes[offset + 66],
                ReadInt32(directoryBytes, offset + 116),
                ReadInt64(directoryBytes, offset + 120)));
        }
        return entries;
    }

    private static byte[] ReadSector(Stream stream, int sectorSize, int sectorIndex)
    {
        var buffer = new byte[sectorSize];
        stream.Position = 512L + (long)sectorIndex * sectorSize;
        var read = stream.Read(buffer);
        if (read != sectorSize)
            throw new InvalidDataException();
        return buffer;
    }

    private static void AddSectorIndex(List<int> sectors, int sector)
    {
        if (sector >= 0)
            sectors.Add(sector);
    }

    private static int ReadInt32(byte[] bytes, int offset) => BitConverter.ToInt32(bytes, offset);
    private static long ReadInt64(byte[] bytes, int offset) => BitConverter.ToInt64(bytes, offset);
    private static ushort ReadUInt16(byte[] bytes, int offset) => BitConverter.ToUInt16(bytes, offset);

    private sealed record DirectoryEntry(string Name, byte ObjectType, int StartSector, long Size);

    private sealed record CompoundFile(
        Stream Stream,
        int SectorSize,
        int MiniSectorSize,
        int MiniStreamCutoffSize,
        int[] Fat,
        int[] MiniFat,
        byte[] MiniStream,
        List<DirectoryEntry> Entries);
}
