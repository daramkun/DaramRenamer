using System.Collections.Immutable;
using System.Text;

namespace DaramRenamer.Helpers;

public class TextFileHelper
{
    private static readonly ImmutableDictionary<byte[], Encoding> BomAndEncodings;

    static TextFileHelper()
    {
        BomAndEncodings = new Dictionary<byte[], Encoding>
        {
            {[0xEF, 0xBB, 0xBF], new UTF8Encoding()},
            {[0xFE, 0xFF], new UnicodeEncoding(true, false)},
            {[0xFF, 0xFE], new UnicodeEncoding(false, true)},
            {[0x00, 0x00, 0xFE, 0xFF], new UTF32Encoding(true, false)},
            {[0xFF, 0xFE, 0x00, 0x00], new UTF32Encoding(false, false)},
            {[0x2B, 0x2F, 0x76], new UTF7Encoding()},
        }.ToImmutableDictionary();
    }

    public static Encoding DetectEncoding(Span<byte> buffer)
    {
        foreach (var (bom, encoding) in BomAndEncodings)
        {
            if (buffer.StartsWith(bom))
                return encoding;
        }

        return Encoding.Default;
    }

    public static bool IsTextFile(string path, int windowSize = 4096)
    {
        using var fileStream = File.OpenRead(path);

        Span<byte> rawData = stackalloc byte[windowSize];
        Span<char> text = stackalloc char[windowSize];
        var isText = false;

        var rawLength = fileStream.Read(rawData);
        fileStream.Seek(0, SeekOrigin.Begin);

        var encoding = DetectEncoding(rawData);

        using (var reader = new StreamReader(fileStream))
        {
            reader.Read(text);
        }

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, encoding);
        writer.Write(text);
        writer.Flush();

        var memoryBuffer = memoryStream.GetBuffer();
        for (var i = 0; i < rawLength && isText; ++i)
            isText = rawData[i] == memoryBuffer[i];

        return isText;
    }
}