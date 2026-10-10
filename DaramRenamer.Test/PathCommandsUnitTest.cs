using DaramRenamer.Commands;

namespace DaramRenamer.Test;

[TestClass]
public class PathCommandsUnitTest
{
    [TestMethod]
    public void AbsolutePathCommandSetsFullTargetWhenCurrentPathExists()
    {
        using var folder = new TemporaryFolder();
        var current = Path.Combine(folder.Root, "current");
        var target = Path.Combine(folder.Root, "target");
        Directory.CreateDirectory(current);
        var item = CommandTestHelpers.CreateItem(
            "name.txt",
            sourceFullPath: Path.Combine(current, "name.txt"),
            changedPath: current);

        new AbsolutePathCommand { Path = target }.Apply(item, 0);

        Assert.AreEqual(Path.GetFullPath(target), item.ChangedPath);
    }

    [TestMethod]
    public void AbsolutePathCommandLeavesPathWhenCurrentPathDoesNotExist()
    {
        using var folder = new TemporaryFolder();
        var missing = Path.Combine(folder.Root, "missing");
        var item = CommandTestHelpers.CreateItem(
            "name.txt",
            sourceFullPath: Path.Combine(missing, "name.txt"),
            changedPath: missing);

        new AbsolutePathCommand { Path = folder.Root }.Apply(item, 0);

        Assert.AreEqual(missing, item.ChangedPath);
    }

    [TestMethod]
    public void RelativePathCommandCollapsesParentSegment()
    {
        var changedPath = Path.Combine("root", "sub");
        var item = CommandTestHelpers.CreateItem("name.txt", changedPath: changedPath);

        new RelativePathCommand { Path = $"..{Path.DirectorySeparatorChar}target" }.Apply(item, 0);

        Assert.AreEqual(Path.Combine("root", "target"), item.ChangedPath);
    }

    [TestMethod]
    public void RelativePathCommandLeavesPathUnchangedWhenItWouldEscapeRoot()
    {
        var changedPath = Path.Combine("root");
        var item = CommandTestHelpers.CreateItem("name.txt", changedPath: changedPath);

        new RelativePathCommand
        {
            Path = $"..{Path.DirectorySeparatorChar}.."
        }.Apply(item, 0);

        Assert.AreEqual(changedPath, item.ChangedPath);
    }
}
