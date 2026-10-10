using DaramRenamer.Commands;

namespace DaramRenamer.Test;

[TestClass]
public class FilenameCommandsUnitTest
{
    [TestMethod]
    public void ManualEditCommandSetsNameAndPath()
    {
        var item = CommandTestHelpers.CreateItem("old.txt");
        var command = new ManualEditCommand { ChangeName = "new.md", ChangePath = "target" };

        command.Apply(item, 0);

        Assert.AreEqual("new.md", item.ChangedName);
        Assert.AreEqual("target", item.ChangedPath);
    }

    [TestMethod]
    public void ReplaceRegexCommandUsesCaptureGroupsAndPreservesExtension()
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "42-alpha.txt");
        var command = new ReplaceRegexCommand
        {
            Find = @"(\d+)-(\w+)",
            Replace = "$2_$1"
        };

        command.Apply(item, 0);

        Assert.AreEqual("alpha_42.txt", item.ChangedName);
    }

    [TestMethod]
    public void ReplaceRegexCommandRebuildsCachedPatternWhenFindChanges()
    {
        var command = new ReplaceRegexCommand { Find = "a", Replace = "x" };
        var first = CommandTestHelpers.CreateItem("first.txt", changedName: "aba.txt");
        command.Apply(first, 0);
        command.Find = "b";
        var second = CommandTestHelpers.CreateItem("second.txt", changedName: "aba.txt");

        command.Apply(second, 0);

        Assert.AreEqual("xbx.txt", first.ChangedName);
        Assert.AreEqual("axa.txt", second.ChangedName);
    }

    [TestMethod]
    public void ArrangeRegexCommandReordersGroupsAndPreservesExtension()
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "alpha-42.txt");
        var command = new ArrangeRegexCommand
        {
            Regex = @"(\w+)-(\d+)",
            FormatString = "{2}_{1}"
        };

        command.Apply(item, 0);

        Assert.AreEqual("42_alpha.txt", item.ChangedName);
    }

    [TestMethod]
    [DataRow(Position3.Begin, "-name.txt")]
    [DataRow(Position3.End, "name-.txt")]
    [DataRow(Position3.Both, "-name-.txt")]
    public void ConcatCommandPlacesText(Position3 position, string expected)
    {
        var item = CommandTestHelpers.CreateItem("name.txt");
        var command = new ConcatCommand { Text = "-", Position = position };

        command.Apply(item, 0);

        Assert.AreEqual(expected, item.ChangedName);
    }

    [TestMethod]
    public void ConcatCommandCanIncludeExtensionInConcatenation()
    {
        var item = CommandTestHelpers.CreateItem("name.txt");
        var command = new ConcatCommand
        {
            Text = "-copy",
            Position = Position3.End,
            IncludeExtension = true
        };

        command.Apply(item, 0);

        Assert.AreEqual("name.txt-copy", item.ChangedName);
    }

    [TestMethod]
    public void ConcatDirectoryCommandUsesChangedDirectoryName()
    {
        var item = CommandTestHelpers.CreateItem(
            "name.txt",
            changedPath: Path.Combine("root", "folder"));

        new ConcatDirectoryCommand().Apply(item, 0);

        Assert.AreEqual("foldername.txt", item.ChangedName);
    }

    [TestMethod]
    public void ConcatDirectoryCommandSkipsDirectoriesUnlessEnabled()
    {
        var item = CommandTestHelpers.CreateItem(
            "folder",
            isDirectory: true,
            changedPath: Path.Combine("root", "parent"));
        var command = new ConcatDirectoryCommand();

        command.Apply(item, 0);
        Assert.AreEqual("folder", item.ChangedName);

        command.ApplyToDirectory = true;
        command.Apply(item, 0);
        Assert.AreEqual("parentfolder", item.ChangedName);
    }

    [TestMethod]
    [DataRow(Position3.Begin, "name  .txt")]
    [DataRow(Position3.End, "  name.txt")]
    [DataRow(Position3.Both, "name.txt")]
    public void TrimCommandTrimsSelectedSideAndPreservesExtension(Position3 position, string expected)
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "  name  .txt");

        new TrimCommand { Position = position }.Apply(item, 0);

        Assert.AreEqual(expected, item.ChangedName);
    }

    [TestMethod]
    public void DeleteBlockCommandDeletesOneOrAllDelimitedBlocks()
    {
        var command = new DeleteBlockCommand { StartBlock = "[", EndBlock = "]" };
        var one = CommandTestHelpers.CreateItem("source.txt", changedName: "a[x]b[y]c.txt");
        var all = CommandTestHelpers.CreateItem("source.txt", changedName: "a[x]b[y]c.txt");

        command.Apply(one, 0);
        command.DeleteAllBlocks = true;
        command.Apply(all, 0);

        Assert.AreEqual("ab[y]c.txt", one.ChangedName);
        Assert.AreEqual("abc.txt", all.ChangedName);
    }

    [TestMethod]
    public void DeleteCommandKeepsOnlyExtension()
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "renamed.txt");

        new DeleteCommand().Apply(item, 0);

        Assert.AreEqual(".txt", item.ChangedName);
    }

    [TestMethod]
    public void SubstringCommandUsesRemainingTextWhenLengthExceedsName()
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "abcdef.txt");

        new SubstringCommand { StartIndex = 4, Length = 20 }.Apply(item, 0);

        Assert.AreEqual("ef.txt", item.ChangedName);
    }

    [TestMethod]
    public void SubstringCommandLeavesNameUnchangedWhenStartIsPastEnd()
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "abc.txt");

        new SubstringCommand { StartIndex = 3 }.Apply(item, 0);

        Assert.AreEqual("abc.txt", item.ChangedName);
    }

    [TestMethod]
    [DataRow(Convension.Upper, "HELLO WORLD")]
    [DataRow(Convension.Lower, "hello world")]
    [DataRow(Convension.UpperFirstLetter, "Hello World")]
    [DataRow(Convension.SnakeCase, "hello_world")]
    [DataRow(Convension.UpperSnakeCase, "HELLO_WORLD")]
    [DataRow(Convension.KebabCase, "hello-world")]
    [DataRow(Convension.UpperKebabCase, "HELLO-WORLD")]
    [DataRow(Convension.CamelCase, "helloWorld")]
    [DataRow(Convension.PascalCase, "HelloWorld")]
    public void ConvensionCommandAppliesSelectedCaseAndPreservesExtension(
        Convension convension,
        string expected)
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "hello world.txt");

        new ConvensionCommand { Convension = convension }.Apply(item, 0);

        Assert.AreEqual($"{expected}.txt", item.ChangedName);
    }
}
