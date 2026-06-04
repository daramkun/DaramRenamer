using System.Text;

namespace DaramRenamer.FileTypes;

public static class FileTypeDetector
{
    private static readonly (byte[] Signature, string Extension)[] Signatures =
    [
        ([0xFF, 0xD8, 0xFF], "jpg"),
        ([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], "png"),
        ([0x47, 0x49, 0x46, 0x38], "gif"),
        ([0x42, 0x4D], "bmp"),
        ([0x25, 0x50, 0x44, 0x46], "pdf"),
        ([0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C], "7z"),
        ([0x52, 0x61, 0x72, 0x21, 0x1A, 0x07], "rar"),
        ([0x1F, 0x8B], "gz"),
        ([0x42, 0x5A, 0x68], "bz2"),
        ([0x4F, 0x67, 0x67, 0x53], "ogg"),
        ([0x66, 0x4C, 0x61, 0x43], "flac"),
        ([0x49, 0x44, 0x33], "mp3"),
        ([0x52, 0x49, 0x46, 0x46], "riff"),
        ([0x50, 0x4B, 0x03, 0x04], "zip")
    ];

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
            Span<byte> header = stackalloc byte[64];
            stream.Position = 0;
            var read = stream.Read(header);

            if (read >= 12 && Encoding.ASCII.GetString(header[4..8]) == "ftyp")
                return new FileTypeDetectionResult(DetectIsoBaseMediaExtension(header[..read]));

            foreach (var (signature, extension) in Signatures)
                if (read >= signature.Length && header[..signature.Length].SequenceEqual(signature))
                    return extension switch
                    {
                        "zip" => new FileTypeDetectionResult(DetectZipExtension(stream)),
                        "riff" => new FileTypeDetectionResult(DetectRiffExtension(header[..read])),
                        _ => new FileTypeDetectionResult(extension)
                    };

            if (LooksLikeText(stream))
                return new FileTypeDetectionResult("txt");

            return null;
        }
        finally
        {
            stream.Position = origin;
        }
    }

    private static string DetectIsoBaseMediaExtension(ReadOnlySpan<byte> header)
    {
        var brand = Encoding.ASCII.GetString(header[8..Math.Min(header.Length, 12)]);
        return brand switch
        {
            "M4A " or "M4B " => "m4a",
            "M4V " => "m4v",
            "qt  " => "mov",
            _ => "mp4"
        };
    }

    private static string DetectRiffExtension(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 12)
        {
            var form = Encoding.ASCII.GetString(header[8..12]);
            return form switch
            {
                "WAVE" => "wav",
                "AVI " => "avi",
                "WEBP" => "webp",
                _ => "riff"
            };
        }

        return "riff";
    }

    private static string DetectZipExtension(Stream stream)
    {
        try
        {
            using var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read, true);
            var names = archive.Entries.Select(entry => entry.FullName).ToArray();
            if (names.Contains("[Content_Types].xml"))
            {
                if (names.Any(name => name.StartsWith("word/", StringComparison.OrdinalIgnoreCase)))
                    return "docx";
                if (names.Any(name => name.StartsWith("xl/", StringComparison.OrdinalIgnoreCase)))
                    return "xlsx";
                if (names.Any(name => name.StartsWith("ppt/", StringComparison.OrdinalIgnoreCase)))
                    return "pptx";
            }

            if (names.Contains("mimetype"))
            {
                var mimetype = archive.GetEntry("mimetype");
                if (mimetype != null)
                {
                    using var reader = new StreamReader(mimetype.Open(), Encoding.ASCII);
                    return reader.ReadToEnd() switch
                    {
                        "application/vnd.oasis.opendocument.text" => "odt",
                        "application/vnd.oasis.opendocument.spreadsheet" => "ods",
                        "application/vnd.oasis.opendocument.presentation" => "odp",
                        _ => "zip"
                    };
                }
            }
        }
        catch
        {
            // Fall back to the container extension.
        }

        return "zip";
    }

    private static bool LooksLikeText(Stream stream)
    {
        var origin = stream.Position;
        try
        {
            Span<byte> buffer = stackalloc byte[512];
            stream.Position = 0;
            var read = stream.Read(buffer);
            if (read == 0)
                return false;

            for (var i = 0; i < read; ++i)
            {
                var b = buffer[i];
                if (b == 0)
                    return false;
                if (b < 0x08 || b is > 0x0D and < 0x20)
                    return false;
            }

            return true;
        }
        finally
        {
            stream.Position = origin;
        }
    }
}
