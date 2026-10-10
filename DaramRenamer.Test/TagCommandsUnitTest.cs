using System.IO.Compression;
using System.Text;
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
}
