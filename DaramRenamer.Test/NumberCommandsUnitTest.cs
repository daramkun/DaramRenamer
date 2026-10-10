using DaramRenamer.Commands;

namespace DaramRenamer.Test;

[TestClass]
public class NumberCommandsUnitTest
{
    [TestMethod]
    [DataRow(Position3.Begin, 0, "1name.txt")]
    [DataRow(Position3.End, 4, "name5.txt")]
    [DataRow(Position3.Both, 2, "3name3.txt")]
    public void AddIndexCommandUsesOneBasedIndexAndSelectedPosition(
        Position3 position,
        int index,
        string expected)
    {
        var item = CommandTestHelpers.CreateItem("name.txt");

        new AddIndexCommand { Position = position }.Apply(item, index);

        Assert.AreEqual(expected, item.ChangedName);
    }

    [TestMethod]
    public void DeleteNoNumberCommandCollectsDigitsAcrossFilename()
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "A12-03.txt");

        new DeleteNoNumberCommand().Apply(item, 0);

        Assert.AreEqual("1203.txt", item.ChangedName);
    }

    [TestMethod]
    public void DeleteNoNumberCommandCanSeparateDigitGroups()
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "A12-03.txt");

        new DeleteNoNumberCommand { Wordly = true }.Apply(item, 0);

        Assert.AreEqual("12 03.txt", item.ChangedName);
    }

    [TestMethod]
    [DataRow(Position2.Begin, "12abc-5.txt", "13abc-5.txt")]
    [DataRow(Position2.End, "file-009.txt", "file-010.txt")]
    public void IncreaseCommandIncrementsAdjacentNumberRun(
        Position2 position,
        string name,
        string expected)
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: name);

        new IncreaseCommand { Position = position, Count = 1 }.Apply(item, 0);

        Assert.AreEqual(expected, item.ChangedName);
    }

    [TestMethod]
    public void IncreaseCommandLeavesNameUnchangedWithoutNumberOrWithZeroCount()
    {
        var noNumber = CommandTestHelpers.CreateItem("source.txt", changedName: "file.txt");
        var zeroCount = CommandTestHelpers.CreateItem("source.txt", changedName: "file-009.txt");

        new IncreaseCommand().Apply(noNumber, 0);
        new IncreaseCommand { Count = 0 }.Apply(zeroCount, 0);

        Assert.AreEqual("file.txt", noNumber.ChangedName);
        Assert.AreEqual("file-009.txt", zeroCount.ChangedName);
    }

    [TestMethod]
    [DataRow(Position2.Begin, "7-cat.txt", "0007-cat.txt")]
    [DataRow(Position2.End, "cat-7.txt", "cat-0007.txt")]
    public void SameNumberCountCommandPadsSelectedNumberRun(
        Position2 position,
        string name,
        string expected)
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: name);

        new SameNumberCountCommand { Position = position, Count = 4 }.Apply(item, 0);

        Assert.AreEqual(expected, item.ChangedName);
    }

    [TestMethod]
    public void SameNumberCountCommandLeavesNameWithoutNumberUnchanged()
    {
        var item = CommandTestHelpers.CreateItem("source.txt", changedName: "cat.txt");

        new SameNumberCountCommand { Count = 4 }.Apply(item, 0);

        Assert.AreEqual("cat.txt", item.ChangedName);
    }
}
