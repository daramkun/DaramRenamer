using System.IO.Compression;
using System.Text;
using System.Buffers.Binary;
using DaramRenamer.Commands;

namespace DaramRenamer.Test;

[TestClass]
public class TagCommandsUnitTest
{
    [TestMethod]
    public void AddDocumentTagCommandReadsTitleAndAuthorFromOpenXmlMetadata()
    {
        using var folder = new TemporaryFolder();
        var path = WriteDocx(folder, "A title", "A creator");
        var titleItem = CommandTestHelpers.CreateItem(
            "report.docx",
            sourceFullPath: path,
            changedName: "report.docx");
        var authorItem = CommandTestHelpers.CreateItem(
            "report.docx",
            sourceFullPath: path,
            changedName: "report.docx");

        new AddDocumentTagCommand
        {
            Tag = DocumentTag.Title,
            Position = Position3.Begin
        }.Apply(titleItem, 0);
        new AddDocumentTagCommand
        {
            Tag = DocumentTag.Author,
            Position = Position3.End
        }.Apply(authorItem, 0);

        Assert.AreEqual("A titlereport.docx", titleItem.ChangedName);
        Assert.AreEqual("reportA creator.docx", authorItem.ChangedName);
    }

    [TestMethod]
    public void AddDocumentTagCommandSkipsDirectories()
    {
        var item = CommandTestHelpers.CreateItem("folder", isDirectory: true);

        new AddDocumentTagCommand().Apply(item, 0);

        Assert.AreEqual("folder", item.ChangedName);
    }

    [TestMethod]
    public void AddHashCommandComputesKnownSha256AndAddsItBeforeExtension()
    {
        using var folder = new TemporaryFolder();
        var source = folder.WriteFile("input.bin", Encoding.ASCII.GetBytes("abc"));
        var item = CommandTestHelpers.CreateItem(
            "input.bin",
            sourceFullPath: source,
            changedName: "renamed.txt");

        new AddHashCommand
        {
            HashKind = HashKind.SHA256,
            Position = Position3.End
        }.Apply(item, 0);

        Assert.AreEqual(
            "renamedba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad.txt",
            item.ChangedName);
        Assert.AreEqual(
            "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
            AddHashCommand.ComputeHash(HashKind.SHA256, source));
    }

    [TestMethod]
    public void AddHashCommandSkipsDirectories()
    {
        var item = CommandTestHelpers.CreateItem("folder", isDirectory: true);

        new AddHashCommand().Apply(item, 0);

        Assert.AreEqual("folder", item.ChangedName);
    }

    [TestMethod]
    public void AddMediaTagCommandUnicodeConversionPreservesOrdinaryTextAndSkipsDirectories()
    {
        var item = CommandTestHelpers.CreateItem("folder", isDirectory: true);

        new AddMediaTagCommand().Apply(item, 0);

        Assert.AreEqual("plain text", AddMediaTagCommand.ConvertUnicodeText("plain text"));
        Assert.AreEqual("folder", item.ChangedName);
    }

    [TestMethod]
    public void MediaTagReaderReadsImageFormatAndDimensions()
    {
        using var folder = new TemporaryFolder();
        var path = folder.WriteFile(
            "one-pixel.png",
            Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII="));

        Assert.AreEqual("PNG", MediaTagReader.Read(path, MediaTag.ImageCodec));
        Assert.AreEqual("1", MediaTagReader.Read(path, MediaTag.ImageWidth));
        Assert.AreEqual("1", MediaTagReader.Read(path, MediaTag.ImageHeight));
        Assert.AreEqual(string.Empty, MediaTagReader.Read(path, MediaTag.ImageQuality));
    }

    [TestMethod]
    public void MediaTagReaderReadsPcmWaveProperties()
    {
        using var folder = new TemporaryFolder();
        var path = WriteWave(folder);

        Assert.AreEqual("8000", MediaTagReader.Read(path, MediaTag.AudioSamplerate));
        Assert.AreEqual("1", MediaTagReader.Read(path, MediaTag.AudioChannels));
        Assert.AreEqual("8", MediaTagReader.Read(path, MediaTag.AudioBitsPerSample));
        Assert.AreEqual("00:00:01", MediaTagReader.Read(path, MediaTag.AudioDuration));
    }

    [TestMethod]
    public void MediaTagReaderReadsMatroskaVideoAndNestedTagsWithoutDecodingFrames()
    {
        using var folder = new TemporaryFolder();
        var path = folder.WriteFile("sample.mkv", CreateMatroskaFile());

        Assert.AreEqual("Tagged title", MediaTagReader.Read(path, MediaTag.VideoTitle));
        Assert.AreEqual("00:00:12", MediaTagReader.Read(path, MediaTag.VideoDuration));
        Assert.AreEqual("1920", MediaTagReader.Read(path, MediaTag.VideoWidth));
        Assert.AreEqual("1080", MediaTagReader.Read(path, MediaTag.VideoHeight));
        Assert.AreEqual("H.264/AVC", MediaTagReader.Read(path, MediaTag.VideoCodec));
        Assert.AreEqual("Sci-Fi,Drama", MediaTagReader.Read(path, MediaTag.VideoGenres));
        Assert.AreEqual("Drama", MediaTagReader.Read(path, MediaTag.VideoGenres, 1));
        Assert.AreEqual("2024", MediaTagReader.Read(path, MediaTag.VideoYear));
        Assert.AreEqual("Studio", MediaTagReader.Read(path, MediaTag.VideoCopyright));
    }

    [TestMethod]
    public void AddGitInfoCommandLeavesNameUnchangedOutsideGitRepository()
    {
        using var folder = new TemporaryFolder();
        var item = CommandTestHelpers.CreateItem(
            "folder",
            isDirectory: true,
            sourceFullPath: folder.Root,
            changedName: "folder");

        new AddGitInfoCommand().Apply(item, 0);

        Assert.AreEqual("folder", item.ChangedName);
    }

    private static string WriteDocx(TemporaryFolder folder, string title, string author)
    {
        var path = Path.Combine(folder.Root, "metadata.docx");
        using var stream = File.Create(path);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        var entry = archive.CreateEntry("docProps/core.xml");
        using var writer = new StreamWriter(entry.Open());
        writer.Write(
            "<cp:coreProperties xmlns:cp=\"urn:cp\" xmlns:dc=\"urn:dc\">" +
            $"<dc:title>{title}</dc:title><dc:creator>{author}</dc:creator>" +
            "</cp:coreProperties>");
        return path;
    }

    private static string WriteWave(TemporaryFolder folder)
    {
        const int sampleRate = 8000;
        const int channels = 1;
        const short bitsPerSample = 8;
        var sampleData = new byte[sampleRate];
        var path = Path.Combine(folder.Root, "one-second.wav");

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + sampleData.Length);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(sampleData.Length);
        writer.Write(sampleData);

        return path;
    }

    private static byte[] CreateMatroskaFile()
    {
        var ebmlHeader = EbmlMaster(0x1A45DFA3,
            EbmlUnsigned(0x4286, 1),
            EbmlUnsigned(0x42F7, 1),
            EbmlUnsigned(0x42F2, 4),
            EbmlUnsigned(0x42F3, 8),
            EbmlString(0x4282, "matroska"),
            EbmlUnsigned(0x4287, 4),
            EbmlUnsigned(0x4285, 2));

        var info = EbmlMaster(0x1549A966,
            EbmlUnsigned(0x2AD7B1, 1_000_000),
            EbmlFloat(0x4489, 12_500),
            EbmlString(0x4D80, "Test muxer"),
            EbmlString(0x5741, "Test writer"));

        var alternateTrack = EbmlMaster(0xAE,
            EbmlUnsigned(0xD7, 1),
            EbmlUnsigned(0x73C5, 1),
            EbmlUnsigned(0x83, 1),
            EbmlUnsigned(0x88, 0),
            EbmlString(0x86, "V_VP9"),
            EbmlMaster(0xE0, EbmlUnsigned(0xB0, 640), EbmlUnsigned(0xBA, 360)));
        var defaultTrack = EbmlMaster(0xAE,
            EbmlUnsigned(0xD7, 2),
            EbmlUnsigned(0x73C5, 2),
            EbmlUnsigned(0x83, 1),
            EbmlUnsigned(0x88, 1),
            EbmlString(0x86, "V_MPEG4/ISO/AVC"),
            EbmlMaster(0xE0, EbmlUnsigned(0xB0, 1920), EbmlUnsigned(0xBA, 1080)));
        var tracks = EbmlMaster(0x1654AE6B, alternateTrack, defaultTrack);

        var tags = EbmlMaster(0x1254C367,
            EbmlMaster(0x7373,
                EbmlMaster(0x63C0),
                EbmlMaster(0x67C8, EbmlString(0x45A3, "TITLE"), EbmlString(0x4487, "Tagged title")),
                EbmlMaster(0x67C8, EbmlString(0x45A3, "GENRE"), EbmlString(0x4487, "Sci-Fi")),
                EbmlMaster(0x67C8, EbmlString(0x45A3, "GENRE"), EbmlString(0x4487, "Drama")),
                EbmlMaster(0x67C8, EbmlString(0x45A3, "DATE_RELEASED"), EbmlString(0x4487, "2024-02-03")),
                EbmlMaster(0x67C8, EbmlString(0x45A3, "COPYRIGHT"), EbmlString(0x4487, "Studio"))));

        // An unknown-sized Cluster must be skipped until the next Segment-level element.
        var unknownSizedCluster = Combine(
            EbmlId(0x1F43B675),
            [0xFF],
            EbmlUnsigned(0xE7, 0));
        var segment = Combine(
            EbmlId(0x18538067),
            [0xFF],
            Combine(info, tracks, unknownSizedCluster, tags));
        return Combine(ebmlHeader, segment);
    }

    private static byte[] EbmlMaster(uint id, params byte[][] children) =>
        EbmlElement(id, Combine(children));

    private static byte[] EbmlString(uint id, string value) =>
        EbmlElement(id, Encoding.UTF8.GetBytes(value));

    private static byte[] EbmlUnsigned(uint id, ulong value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(bytes, value);
        var firstNonZero = 0;
        while (firstNonZero < bytes.Length - 1 && bytes[firstNonZero] == 0)
            firstNonZero++;
        return EbmlElement(id, bytes[firstNonZero..].ToArray());
    }

    private static byte[] EbmlFloat(uint id, double value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, BitConverter.DoubleToInt64Bits(value));
        return EbmlElement(id, bytes.ToArray());
    }

    private static byte[] EbmlElement(uint id, byte[] data) =>
        Combine(EbmlId(id), EbmlSize(data.Length), data);

    private static byte[] EbmlId(uint id)
    {
        var length = id > 0xFFFFFF ? 4 : id > 0xFFFF ? 3 : id > 0xFF ? 2 : 1;
        var result = new byte[length];
        for (var index = 0; index < length; index++)
            result[length - index - 1] = (byte)(id >> (index * 8));
        return result;
    }

    private static byte[] EbmlSize(int size)
    {
        for (var length = 1; length <= 8; length++)
        {
            var payloadBits = length * 7;
            var maximum = (1UL << payloadBits) - 1;
            if ((ulong)size >= maximum)
                continue;

            var encoded = (ulong)(uint)size | (1UL << payloadBits);
            var result = new byte[length];
            for (var index = 0; index < length; index++)
                result[length - index - 1] = (byte)(encoded >> (index * 8));
            return result;
        }

        throw new ArgumentOutOfRangeException(nameof(size));
    }

    private static byte[] Combine(params byte[][] parts)
    {
        var result = new byte[parts.Sum(part => part.Length)];
        var offset = 0;
        foreach (var part in parts)
        {
            part.CopyTo(result, offset);
            offset += part.Length;
        }

        return result;
    }
}
