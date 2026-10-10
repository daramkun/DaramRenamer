using DaramRenamer.Commands;

namespace DaramRenamer.Test;

[TestClass]
public class ExtensionCommandsUnitTest
{
    [TestMethod]
    [DataRow("md", "name.txt.md")]
    [DataRow(".md", "name.txt.md")]
    public void AddExtensionCommandAppendsExtension(string extension, string expected)
    {
        var item = CommandTestHelpers.CreateItem("name.txt");

        new AddExtensionCommand { Extension = extension }.Apply(item, 0);

        Assert.AreEqual(expected, item.ChangedName);
    }

    [TestMethod]
    public void AddExtensionCommandSkipsDirectoriesUnlessEnabled()
    {
        var item = CommandTestHelpers.CreateItem("folder", isDirectory: true);
        var command = new AddExtensionCommand { Extension = ".bak" };

        command.Apply(item, 0);
        Assert.AreEqual("folder", item.ChangedName);

        command.ApplyToDirectory = true;
        command.Apply(item, 0);
        Assert.AreEqual("folder.bak", item.ChangedName);
    }

    [TestMethod]
    public void AddExtensionAutoCommandUsesDetectedTypeAndSkipsUnknownContent()
    {
        using var folder = new TemporaryFolder();
        var png = folder.WriteFile("image.bin", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        var unknown = folder.WriteFile("unknown.bin", [0x00, 0xFF, 0x00]);
        var pngItem = CommandTestHelpers.CreateItem("image.bin", sourceFullPath: png, changedName: "image.bin");
        var unknownItem = CommandTestHelpers.CreateItem("unknown.bin", sourceFullPath: unknown);

        new AddExtensionAutoCommand().Apply(pngItem, 0);
        new AddExtensionAutoCommand().Apply(unknownItem, 0);

        Assert.AreEqual("image.bin.png", pngItem.ChangedName);
        Assert.AreEqual("unknown.bin", unknownItem.ChangedName);
    }

    [TestMethod]
    public void AddExtensionAutoCommandSkipsDirectories()
    {
        using var folder = new TemporaryFolder();
        var item = CommandTestHelpers.CreateItem(
            "folder",
            isDirectory: true,
            sourceFullPath: folder.Root);

        new AddExtensionAutoCommand().Apply(item, 0);

        Assert.AreEqual("folder", item.ChangedName);
    }

    [TestMethod]
    public void DeleteExtensionCommandRemovesExtensionAndHonorsDirectoryOption()
    {
        var file = CommandTestHelpers.CreateItem("source.txt", changedName: "renamed.txt");
        var directory = CommandTestHelpers.CreateItem("folder.txt", isDirectory: true);
        var command = new DeleteExtensionCommand();

        command.Apply(file, 0);
        command.Apply(directory, 0);
        Assert.AreEqual("renamed", file.ChangedName);
        Assert.AreEqual("folder.txt", directory.ChangedName);

        command.ApplyToDirectory = true;
        command.Apply(directory, 0);
        Assert.AreEqual("folder", directory.ChangedName);
    }

    [TestMethod]
    [DataRow("name.jpeg", ConvensionBinary.Upper, "name.JPEG")]
    [DataRow("name.JPEG", ConvensionBinary.Lower, "name.jpeg")]
    public void ConvensionExtensionCommandChangesOnlyExtension(
        string name,
        ConvensionBinary convension,
        string expected)
    {
        var item = CommandTestHelpers.CreateItem(name);

        new ConvensionExtensionCommand { Convension = convension }.Apply(item, 0);

        Assert.AreEqual(expected, item.ChangedName);
    }

    [TestMethod]
    public void ConvensionExtensionCommandSkipsDirectoriesByDefault()
    {
        var item = CommandTestHelpers.CreateItem("folder.JPG", isDirectory: true);
        var command = new ConvensionExtensionCommand { Convension = ConvensionBinary.Lower };

        command.Apply(item, 0);
        Assert.AreEqual("folder.JPG", item.ChangedName);

        command.ApplyToDirectory = true;
        command.Apply(item, 0);
        Assert.AreEqual("folder.jpg", item.ChangedName);
    }

    [TestMethod]
    [DataRow("md", "name.md")]
    [DataRow(".md", "name.md")]
    public void ReplaceExtensionCommandReplacesExistingExtension(string extension, string expected)
    {
        var item = CommandTestHelpers.CreateItem("name.txt");

        new ReplaceExtensionCommand { Extension = extension }.Apply(item, 0);

        Assert.AreEqual(expected, item.ChangedName);
    }

    [TestMethod]
    public void ReplaceExtensionCommandLeavesDirectoryAloneUnlessEnabled()
    {
        var item = CommandTestHelpers.CreateItem("folder.txt", isDirectory: true);
        var command = new ReplaceExtensionCommand { Extension = ".bak" };

        command.Apply(item, 0);
        Assert.AreEqual("folder.txt", item.ChangedName);

        command.ApplyToDirectory = true;
        command.Apply(item, 0);
        Assert.AreEqual("folder.bak", item.ChangedName);
    }
}
