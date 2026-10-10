using DaramRenamer.Commands;

namespace DaramRenamer.Test;

[TestClass]
public class DateCommandsUnitTest
{
    [TestMethod]
    public void AddDateCommandAddsCreationDateAndKeepsExtension()
    {
        using var folder = new TemporaryFolder();
        var source = folder.WriteFile("photo.txt", [1, 2, 3]);
        File.SetCreationTime(source, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Local));
        var item = CommandTestHelpers.CreateItem(
            "photo.txt",
            sourceFullPath: source,
            changedName: "photo.txt");
        var command = new AddDateCommand
        {
            Kind = FileDateKind.Creation,
            Format = "yyyyMMdd",
            Position = Position3.Begin
        };

        command.Apply(item, 0);

        var expectedDate = File.GetCreationTime(source).ToString("yyyyMMdd");
        Assert.AreEqual($"{expectedDate}photo.txt", item.ChangedName);
    }

    [TestMethod]
    public void AddDateCommandUsesCurrentTimeForNowKind()
    {
        var before = DateTime.Now.ToString("yyyy");
        var item = CommandTestHelpers.CreateItem("photo.txt");

        new AddDateCommand
        {
            Kind = FileDateKind.Now,
            Format = "yyyy",
            Position = Position3.Begin
        }.Apply(item, 0);

        var after = DateTime.Now.ToString("yyyy");
        var actual = item.ChangedName[..4];
        Assert.IsTrue(actual == before || actual == after);
        Assert.AreEqual("photo.txt", item.ChangedName[4..]);
    }

    [TestMethod]
    public void DeleteDateCommandRemovesSupportedDatePatternAndKeepsExtension()
    {
        var item = CommandTestHelpers.CreateItem("source.jpg", changedName: "holiday-20241009.jpg");

        new DeleteDateCommand().Apply(item, 0);

        Assert.AreEqual("holiday-.jpg", item.ChangedName);
    }

    [TestMethod]
    public void DeleteDateCommandLeavesNameWithoutDateUnchanged()
    {
        var item = CommandTestHelpers.CreateItem("photo.jpg");

        new DeleteDateCommand().Apply(item, 0);

        Assert.AreEqual("photo.jpg", item.ChangedName);
    }
}
