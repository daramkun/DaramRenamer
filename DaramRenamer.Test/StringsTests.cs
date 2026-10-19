namespace DaramRenamer.Test;

[TestClass]
public class StringsTests
{
    [TestMethod]
    public void LoadsEveryRowOfTheStringTable()
    {
        using var stream = typeof(Strings).Assembly
            .GetManifestResourceStream("DaramRenamer.Strings.Strings.csv")!;
        using var reader = new StreamReader(stream);
        var keys = new List<string>();
        reader.ReadLine();
        while (reader.ReadLine() is { Length: > 0 } line)
            keys.Add(line[..line.IndexOf(',')]);

        var missing = keys.Where(key => !Strings.Instance.ContainsKey(key)).ToArray();

        Assert.IsEmpty(missing, $"Rows were not loaded: {string.Join(", ", missing.Take(5))}");
    }

    [TestMethod]
    public void QuotedTextIsUnescaped()
    {
        Assert.Contains("\"winget upgrade DaramRenamer.DaramRenamer\"", Strings.Instance["UpdateViaWingetText"]);
    }
}
