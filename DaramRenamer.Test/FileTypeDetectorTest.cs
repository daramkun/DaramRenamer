using System.IO.Compression;
using System.Text;
using DaramRenamer.FileTypes;

namespace DaramRenamer.Test;

[TestClass]
public class FileTypeDetectorTest
{
    [TestMethod]
    public void DetectsSignatureFormats()
    {
        Assert.AreEqual("png", Detect([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]));
        Assert.AreEqual("7z", Detect([0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C]));
        Assert.AreEqual("webp", Detect([0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50]));
    }

    [TestMethod]
    public void DetectsIsoBaseMediaBrands()
    {
        Assert.AreEqual("mp4", Detect(MakeFtyp("isom")));
        Assert.AreEqual("m4a", Detect(MakeFtyp("M4A ")));
        Assert.AreEqual("mov", Detect(MakeFtyp("qt  ")));
    }

    [TestMethod]
    public void DetectsZipDetails()
    {
        Assert.AreEqual("docx", Detect(MakeZip(
            ("[Content_Types].xml", ""),
            ("_rels/.rels", ""),
            ("word/_rels/document.xml.rels", ""))));
        Assert.AreEqual("epub", Detect(MakeZip(("mimetype", "application/epub+zip"))));
        Assert.AreEqual("odt", Detect(MakeZip(("mimetype", "application/vnd.oasis.opendocument.text"))));
    }

    [TestMethod]
    public void DetectsTextDetails()
    {
        Assert.AreEqual("xml", Detect(Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><root />")));
        Assert.AreEqual("config", Detect(Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><configuration />")));
        Assert.AreEqual("reg", Detect(Encoding.UTF8.GetBytes("Windows Registry Editor Version 5.00\n")));
        Assert.AreEqual("sh", Detect(Encoding.UTF8.GetBytes("#!/bin/sh\n")));
    }

    private static string Detect(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return FileTypeDetector.Detect(stream)?.Extension ?? string.Empty;
    }

    private static byte[] MakeFtyp(string brand)
    {
        var bytes = new byte[16];
        bytes[3] = 16;
        Encoding.ASCII.GetBytes("ftyp").CopyTo(bytes, 4);
        Encoding.ASCII.GetBytes(brand).CopyTo(bytes, 8);
        return bytes;
    }

    private static byte[] MakeZip(params (string Name, string Content)[] entries)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var (name, content) in entries)
            {
                var entry = archive.CreateEntry(name);
                using var writer = new StreamWriter(entry.Open(), Encoding.ASCII);
                writer.Write(content);
            }
        }

        return stream.ToArray();
    }
}
