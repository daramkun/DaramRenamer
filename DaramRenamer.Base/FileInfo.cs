namespace DaramRenamer;

[Serializable]
public class FileInfo : FileItem
{
    public FileInfo(string fullPath, bool directoryCheck = true)
        : base(fullPath, directoryCheck)
    {
    }

    public FileInfo(FileInfo file)
        : base(file)
    {
    }

    public FileInfo(string originalFullPath, string changedFilename, string? changedPath = null)
        : base(originalFullPath, changedFilename, changedPath ?? Path.GetDirectoryName(originalFullPath) ?? string.Empty,
            File.GetAttributes(originalFullPath).HasFlag(FileAttributes.Directory))
    {
    }
}
