using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace DaramRenamer.FileTypes;

public static class FileTypeDetector
{
    private const int HeaderSize = 4096;

    private static readonly SignatureRule[] SignatureRules =
    [
        Rule("jpg", 0, [0xFF, 0xD8, 0xFF, 0xFE, 0x00]),
        Rule("jpg", 0, [0xFF, 0xD8, 0xFF, 0xDB]),
        Rule("jpg", 0, [0xFF, 0xD8, 0xFF, 0xE0]),
        Rule("jpg", 0, [0xFF, 0xD8, 0xFF, 0xE1]),
        Rule("jpg", 0, [0xFF, 0xD8, 0xFF, 0xE2]),
        Rule("jpg", 0, [0xFF, 0xD8, 0xFF, 0xE3]),
        Rule("jpg", 0, [0xFF, 0xD8, 0xFF, 0xE8]),
        Rule("png", 0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        Rule("gif", 0, Ascii("GIF87a")),
        Rule("gif", 0, Ascii("GIF89a")),
        Rule("bmp", 0, Ascii("BM"), extra: HasZeroDWordAt6),
        Rule("tif", 0, [0x49, 0x49, 0x49]),
        Rule("tif", 0, [0x49, 0x49, 0x2A, 0x00]),
        Rule("tif", 0, [0x4D, 0x4D, 0x00, 0x2A]),
        Rule("tif", 0, [0x4D, 0x4D, 0x00, 0x2B]),
        Rule("jp2", 0, [0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20]),
        Rule("hdp", 0, [0x49, 0x49, 0xBC, 0x01]),
        Rule("ico", 0, [0x00, 0x00, 0x01, 0x00]),
        Rule("cur", 0, [0x00, 0x00, 0x02, 0x00]),
        Rule("dds", 0, Ascii("DDS ")),
        Rule("psd", 0, Ascii("8BPS")),
        Rule("ktx", 0, [0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A]),
        Rule("pkm", 0, Ascii("PKM 10")),
        Rule("flac", 0, Ascii("fLaC")),
        Rule("ogg", 0, Ascii("OggS")),
        Rule("mp3", 0, [0xFF, 0xFB]),
        Rule("mp3", 0, Ascii("ID3")),
        Rule("mid", 0, Ascii("MThd")),
        Rule("wav", 0, Ascii("RIFF"), extra: header => HasSignature(header, 8, Ascii("WAVE"))),
        Rule("avi", 0, Ascii("RIFF"), extra: header => HasSignature(header, 8, Ascii("AVI "))),
        Rule("webp", 0, Ascii("RIFF"), extra: header => HasSignature(header, 8, Ascii("WEBP"))),
        Rule("flv", 0, Ascii("FLV")),
        Rule("swf", 0, Ascii("CWS")),
        Rule("swf", 0, Ascii("FWS")),
        Rule("webm", 0, [0x1A, 0x45, 0xDF, 0xA3], extra: LooksLikeWebM),
        Rule("7z", 0, [0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C]),
        Rule("rar", 0, [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00]),
        Rule("rar", 0, [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00]),
        Rule("gz", 0, [0x1F, 0x8B]),
        Rule("bz2", 0, Ascii("BZh")),
        Rule("z", 0, [0x1F, 0x9D]),
        Rule("z", 0, [0x1F, 0xA0]),
        Rule("cab", 0, Ascii("MSCF")),
        Rule("lzh", 2, Ascii("-lh")),
        Rule("xar", 0, Ascii("xar!")),
        Rule("rpm", 0, [0xED, 0xAB, 0xEE, 0xDB]),
        Rule("mpq", 0, [0x4D, 0x50, 0x51, 0x1A]),
        Rule("pak", 0, [0x1A, 0x0B]),
        Rule("pak", 0, Ascii("PACK")),
        Rule("zip", 0, [0x50, 0x4B, 0x03, 0x04]),
        Rule("zip", 0, [0x50, 0x4B, 0x05, 0x06]),
        Rule("zip", 0, [0x50, 0x4B, 0x07, 0x08]),
        Rule("pdf", 0, Ascii("%PDF-")),
        Rule("rtf", 0, Ascii(@"{\rtf1")),
        Rule("bplist", 0, Ascii("bplist")),
        Rule("sqlite", 0, [0x53, 0x51, 0x4C, 0x69, 0x74, 0x65, 0x20, 0x66, 0x6F, 0x72, 0x6D, 0x61, 0x74, 0x20, 0x33, 0x00]),
        Rule("class", 0, [0xCA, 0xFE, 0xBA, 0xBE]),
        Rule("pdb", 0, Ascii("Microsoft C/C++ MSF ")),
        Rule("lnk", 0, [0x4C, 0x00, 0x00, 0x00, 0x01, 0x14, 0x02, 0x00]),
        Rule("dmp", 0, [0x4D, 0x44, 0x4D, 0x50, 0x93, 0xA7]),
        Rule("dmp", 0, Ascii("PAGEDU")),
        Rule("pfx", 0, [0x30, 0x82, 0x06]),
        Rule("dmg", 0, [0x78, 0x01, 0x73, 0x0D, 0x62, 0x62, 0x60]),
        Rule("tar", 0x101, [0x75, 0x73, 0x74, 0x61, 0x72, 0x00, 0x30, 0x30]),
        Rule("tar", 0x101, [0x75, 0x73, 0x74, 0x61, 0x72, 0x20, 0x20, 0x00]),
        Rule("iso", 0x8001, Ascii("CD001")),
        Rule("hwp", 0, Ascii("HWP Document File V")),
        Rule("tga", 0, [], extra: LooksLikeTga),
        Rule("exe", 0, Ascii("MZ"), extra: IsPortableExecutable)
    ];

    private static readonly Dictionary<string, string> IsoBaseMediaBrands = new(StringComparer.Ordinal)
    {
        ["3gp"] = "3gp",
        ["isom"] = "mp4",
        ["M4A "] = "m4a",
        ["mp42"] = "m4v",
        ["qt  "] = "mov",
        ["qt"] = "mov"
    };

    private static readonly Dictionary<string, string> Mimetypes = new(StringComparer.Ordinal)
    {
        ["application/epub+zip"] = "epub",
        ["application/vnd.oasis.opendocument.text"] = "odt",
        ["application/vnd.oasis.opendocument.spreadsheet"] = "ods",
        ["application/vnd.oasis.opendocument.presentation"] = "odp",
        ["application/vnd.oasis.opendocument.graphics"] = "odg",
        ["application/vnd.oasis.opendocument.formula"] = "odf"
    };

    public static FileTypeDetectionResult? Detect(Stream stream)
    {
        if (!stream.CanSeek)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return Detect(memoryStream);
        }

        var origin = stream.Position;
        try
        {
            var header = ReadHeader(stream, HeaderSize);

            if (TryDetectIsoBaseMedia(header, out var isoExtension))
                return Result(isoExtension);

            if (IsCompoundFile(header))
                return Result(DetectCompoundExtension(stream) ?? "doc");

            foreach (var rule in SignatureRules)
            {
                if (Matches(header, rule))
                    return Result(rule.Extension == "zip" ? DetectZipExtension(stream) : rule.Extension);
            }

            if (LooksLikePrePosixTar(header))
                return Result("tar");

            if (LooksLikeText(stream, out var text))
                return Result(DetectTextExtension(stream, text));

            return null;
        }
        finally
        {
            stream.Position = origin;
        }
    }

    private static FileTypeDetectionResult Result(string extension) => new(extension);

    private static byte[] ReadHeader(Stream stream, int length)
    {
        stream.Position = 0;
        var buffer = new byte[Math.Min(length, Math.Max((int)Math.Min(stream.Length, length), 0))];
        var read = stream.Read(buffer);
        return read == buffer.Length ? buffer : buffer[..read];
    }

    private static bool Matches(ReadOnlySpan<byte> header, SignatureRule rule)
    {
        if (rule.Signature.Length > 0 && !HasSignature(header, rule.Offset, rule.Signature))
            return false;
        return rule.Extra == null || rule.Extra(header);
    }

    private static string DetectZipExtension(Stream stream)
    {
        try
        {
            stream.Position = 0;
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
            var names = archive.Entries.Select(entry => entry.FullName).ToHashSet(StringComparer.Ordinal);

            var contentTypes = ReadZipEntryText(archive, "[Content_Types].xml") ?? string.Empty;
            if (HasAll(names, "[Content_Types].xml", "_rels/.rels", "word/_rels/document.xml.rels"))
                return "docx";
            if (HasAll(names, "[Content_Types].xml", "_rels/.rels", "xl/_rels/workbook.xml.rels"))
                return "xlsx";
            if (HasAll(names, "[Content_Types].xml", "_rels/.rels", "ppt/_rels/presentation.xml.rels"))
                return contentTypes.Contains("presentationml.slideshow", StringComparison.OrdinalIgnoreCase) ? "ppsx" : "pptx";
            if (HasAll(names, "classes.dex", "AndroidManifest.xml"))
                return "apk";

            var mimetype = ReadZipEntryText(archive, "mimetype");
            if (mimetype != null && Mimetypes.TryGetValue(mimetype, out var extension))
                return extension;
        }
        catch
        {
            // Fall back to the container extension.
        }

        return "zip";
    }

    private static string? ReadZipEntryText(ZipArchive archive, string name)
    {
        var entry = archive.GetEntry(name);
        if (entry == null)
            return null;
        using var stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.ASCII);
        return reader.ReadToEnd();
    }

    private static bool HasAll(ISet<string> names, params string[] required) =>
        required.All(names.Contains);

    private static bool TryDetectIsoBaseMedia(ReadOnlySpan<byte> header, out string extension)
    {
        extension = string.Empty;
        if (header.Length < 12 || !HasSignature(header, 4, Ascii("ftyp")))
            return false;

        var brand = Encoding.ASCII.GetString(header[8..Math.Min(header.Length, 12)]);
        foreach (var (prefix, mapped) in IsoBaseMediaBrands)
        {
            if (!brand.StartsWith(prefix, StringComparison.Ordinal))
                continue;
            extension = mapped;
            return true;
        }

        extension = "mp4";
        return true;
    }

    private static string DetectTextExtension(Stream stream, string text)
    {
        if (text.StartsWith("-----BEGIN CERTIFICATE-----", StringComparison.Ordinal))
            return "crt";
        if (text.StartsWith("#!/", StringComparison.Ordinal))
            return "sh";
        if (text.StartsWith("Windows Registry Editor Version ", StringComparison.Ordinal))
            return "reg";
        if (text.Contains("Microsoft Visual Studio Solution File, Format Version", StringComparison.Ordinal))
            return "sln";
        if (Regex.IsMatch(text, @"^\s*<\?xml", RegexOptions.CultureInvariant))
            return LooksLikeConfigurationXml(stream) ? "config" : "xml";
        if (Regex.IsMatch(text, @"^\[(.+)\]\r?\n", RegexOptions.CultureInvariant))
            return "ini";
        if (Regex.IsMatch(text, @"^\s*\d+\s*\r?\n\d{2}:\d{2}:\d{2}[,.]\d{3}\s+-->\s+\d{2}:\d{2}:\d{2}[,.]\d{3}", RegexOptions.CultureInvariant))
            return "srt";
        return "txt";
    }

    private static bool LooksLikeConfigurationXml(Stream stream)
    {
        var origin = stream.Position;
        try
        {
            stream.Position = 0;
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { CloseInput = false, DtdProcessing = DtdProcessing.Ignore });
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                    return reader.Name == "configuration";
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            stream.Position = origin;
        }

        return false;
    }

    private static bool IsCompoundFile(ReadOnlySpan<byte> header) =>
        HasSignature(header, 0, [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]);

    private static string? DetectCompoundExtension(Stream stream)
    {
        var directoryNames = CompoundFileDirectoryReader.ReadDirectoryNames(stream);
        if (directoryNames.Contains("FileHeader") && CompoundFileDirectoryReader.TryReadStream(stream, "FileHeader", out var fileHeader) &&
            fileHeader.Length >= 18 && Encoding.UTF8.GetString(fileHeader, 0, 18) == "HWP Document File\0")
            return "hwp";
        if (directoryNames.Contains("WordDocument"))
            return "doc";
        if (directoryNames.Contains("Workbook"))
            return "xls";
        if (directoryNames.Contains("PowerPoint Document"))
            return "ppt";
        if (directoryNames.Contains("\u0005SummaryInformation"))
            return "msi";
        return null;
    }

    private static bool IsPortableExecutable(ReadOnlySpan<byte> header)
    {
        if (header.Length < 64)
            return false;
        var peOffset = BitConverter.ToInt32(header[60..64]);
        return peOffset >= 0 && peOffset + 4 <= header.Length && HasSignature(header, peOffset, [0x50, 0x45, 0x00, 0x00]);
    }

    private static bool LooksLikeWebM(ReadOnlySpan<byte> header) =>
        header.Length > 0x1F && header[0x1F] == 0x42;

    private static bool LooksLikeTga(ReadOnlySpan<byte> header)
    {
        if (header.Length < 18)
            return false;
        var imageType = header[3];
        var depth = header[17];
        var colorMapType = header[1];
        return imageType is 0 or 1 or 2 or 3 or 9 or 10 or 11 or 32 or 33 &&
               depth is 8 or 15 or 16 or 24 or 32 &&
               (colorMapType == 0 || colorMapType == 1 && depth == 8);
    }

    private static bool LooksLikePrePosixTar(ReadOnlySpan<byte> header)
    {
        if (header.Length < 108)
            return false;
        var mode = Encoding.ASCII.GetString(header[100..108]);
        return Regex.IsMatch(mode, @"[0-7]{6} \0|[0-7]{7}\0", RegexOptions.CultureInvariant);
    }

    private static bool HasZeroDWordAt6(ReadOnlySpan<byte> header) =>
        HasSignature(header, 6, [0x00, 0x00, 0x00, 0x00]);

    private static bool HasSignature(ReadOnlySpan<byte> header, int offset, ReadOnlySpan<byte> signature)
    {
        return offset >= 0 && header.Length >= offset + signature.Length && header.Slice(offset, signature.Length).SequenceEqual(signature);
    }

    private static bool LooksLikeText(Stream stream, out string text)
    {
        text = string.Empty;
        var origin = stream.Position;
        try
        {
            stream.Position = 0;
            var bytes = new byte[Math.Min(4096, Math.Max((int)Math.Min(stream.Length, 4096), 0))];
            var read = stream.Read(bytes);
            if (read == 0)
                return false;

            var span = bytes.AsSpan(0, read);
            Encoding encoding = DetectEncoding(span, out var bomLength);
            span = span[bomLength..];
            text = encoding.GetString(span);
            return text.All(ch => (!char.IsControl(ch) || ch is '\r' or '\n' or '\t') && ch != '\0');
        }
        catch
        {
            text = string.Empty;
            return false;
        }
        finally
        {
            stream.Position = origin;
        }
    }

    private static Encoding DetectEncoding(ReadOnlySpan<byte> bytes, out int bomLength)
    {
        bomLength = 0;
        if (HasSignature(bytes, 0, [0xEF, 0xBB, 0xBF]))
        {
            bomLength = 3;
            return Encoding.UTF8;
        }
        if (HasSignature(bytes, 0, [0xFE, 0xFF]))
        {
            bomLength = 2;
            return Encoding.BigEndianUnicode;
        }
        if (HasSignature(bytes, 0, [0xFF, 0xFE]))
        {
            bomLength = 2;
            return Encoding.Unicode;
        }
        if (HasSignature(bytes, 0, [0x00, 0x00, 0xFE, 0xFF]))
        {
            bomLength = 4;
            return new UTF32Encoding(true, true);
        }

        return Encoding.UTF8;
    }

    private static SignatureRule Rule(string extension, int offset, byte[] signature, Func<ReadOnlySpan<byte>, bool>? extra = null) =>
        new(extension, offset, signature, extra);

    private static byte[] Ascii(string text) => Encoding.ASCII.GetBytes(text);

    private sealed record SignatureRule(string Extension, int Offset, byte[] Signature, Func<ReadOnlySpan<byte>, bool>? Extra = null);
}
