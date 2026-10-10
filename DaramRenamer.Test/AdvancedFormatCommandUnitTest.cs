using System.IO.Compression;
using DaramRenamer.Commands;

namespace DaramRenamer.Test;

[TestClass]
public class AdvancedFormatCommandUnitTest
{
    [TestMethod]
    public void AdvancedFormatCommandDefaultFormatsPreserveCurrentNameAndPath()
    {
        var item = CommandTestHelpers.CreateItem(
            "old.txt",
            changedName: "renamed.md",
            changedPath: Path.Combine("target", "folder"));

        new AdvancedFormatCommand().Apply(item, 0);

        Assert.AreEqual("renamed.md", item.ChangedName);
        Assert.AreEqual(Path.Combine("target", "folder"), item.ChangedPath);
    }

    [TestMethod]
    public void AdvancedFormatCommandExpandsFileIndexAndPathMacros()
    {
        var sourcePath = Path.Combine("source", "folder");
        var item = CommandTestHelpers.CreateItem(
            "old.txt",
            sourceFullPath: Path.Combine(sourcePath, "old.txt"),
            changedName: "renamed.txt",
            changedPath: Path.Combine("target", "folder"));
        var command = new AdvancedFormatCommand
        {
            FileNameFormatString = "${FileName}_${Index:3}${Extension}",
            PathFormatString = "${CurrentDirectory}/${ProceedDirectory}/${Path}"
        };

        command.Apply(item, 1);

        Assert.AreEqual("old_002.txt", item.ChangedName);
        Assert.AreEqual($"folder/folder/{sourcePath}", item.ChangedPath);
    }

    [TestMethod]
    public void AdvancedFormatCommandKeepsUnknownTokensAsLiteralText()
    {
        var item = CommandTestHelpers.CreateItem("old.txt");
        var command = new AdvancedFormatCommand
        {
            FileNameFormatString = "prefix-${unknown}"
        };

        command.Apply(item, 0);

        Assert.AreEqual("prefix-${unknown}", item.ChangedName);
    }

    [TestMethod]
    public void AdvancedFormatCommandReadsEnvironmentVariable()
    {
        const string variable = "DARAMRENAMER_TEST_FORMAT_VALUE";
        var originalValue = Environment.GetEnvironmentVariable(variable);
        Environment.SetEnvironmentVariable(variable, "test-value");
        try
        {
            var item = CommandTestHelpers.CreateItem("old.txt");
            var command = new AdvancedFormatCommand
            {
                FileNameFormatString = "${env:DARAMRENAMER_TEST_FORMAT_VALUE}"
            };

            command.Apply(item, 0);

            Assert.AreEqual("test-value", item.ChangedName);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, originalValue);
        }
    }

    [TestMethod]
    public void AdvancedFormatCommandExpandsDetectedExtensionAndHashFromSourceFile()
    {
        using var folder = new TemporaryFolder();
        var source = folder.WriteFile("image.bin", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        var item = CommandTestHelpers.CreateItem(
            "image.bin",
            sourceFullPath: source,
            changedName: "image.bin");
        var command = new AdvancedFormatCommand
        {
            FileNameFormatString = "${FoundExtension}-${hash:SHA256}"
        };
        var expectedHash = AddHashCommand.ComputeHash(HashKind.SHA256, source);

        command.Apply(item, 0);

        Assert.AreEqual($"png-{expectedHash}", item.ChangedName);
    }

    [TestMethod]
    public void AdvancedFormatCommandReadsDocumentTag()
    {
        using var folder = new TemporaryFolder();
        var source = Path.Combine(folder.Root, "metadata.docx");
        using (var stream = File.Create(source))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("docProps/core.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(
                "<cp:coreProperties xmlns:cp=\"urn:cp\" xmlns:dc=\"urn:dc\">" +
                "<dc:title>Document title</dc:title>" +
                "</cp:coreProperties>");
        }

        var item = CommandTestHelpers.CreateItem(
            "metadata.docx",
            sourceFullPath: source,
            changedName: "metadata.docx");
        var command = new AdvancedFormatCommand
        {
            FileNameFormatString = "${doc:Title}"
        };

        command.Apply(item, 0);

        Assert.AreEqual("Document title", item.ChangedName);
    }
}
