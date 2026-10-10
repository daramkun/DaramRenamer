namespace DaramRenamer.Test;

internal static class CommandTestHelpers
{
    public static FileItem CreateItem(
        string name,
        bool isDirectory = false,
        string sourceFullPath = null,
        string changedName = null,
        string changedPath = null)
    {
        var defaultDirectory = Path.Combine(Path.GetTempPath(), "DaramRenamer.CommandTests", "source");
        var source = sourceFullPath ?? Path.Combine(defaultDirectory, name);
        return new FileItem(
            source,
            changedName ?? name,
            changedPath ?? Path.GetDirectoryName(source) ?? defaultDirectory,
            isDirectory);
    }
}

internal sealed class TemporaryFolder : IDisposable
{
    public string Root { get; } = Path.Combine(
        Path.GetTempPath(),
        "DaramRenamer.CommandTests",
        Guid.NewGuid().ToString("N"));

    public TemporaryFolder()
    {
        Directory.CreateDirectory(Root);
    }

    public string WriteFile(string name, byte[] contents)
    {
        var path = Path.Combine(Root, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, contents);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
