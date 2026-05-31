namespace DaramRenamer.FileOperators;

public class DefaultFileOperator : IFileOperator
{
    ~DefaultFileOperator() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        
    }
    
    public virtual void Begin() { }
    public virtual void End() { }

    public virtual void Move(string source, string target, bool overwrite) =>
        File.Move(source, target, overwrite);

    public virtual void Copy(string source, string target, bool overwrite) =>
        File.Copy(source, target, overwrite);

    public virtual IEnumerable<string> EnumerateFiles(string path, bool topDirOnly)
    {
        if (!Directory.Exists(path))
            return [];

        return Directory.EnumerateFiles(path, "*",
            topDirOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
    }

    public virtual bool IsFileExists(string path) =>
        File.Exists(path) && File.GetAttributes(path) != FileAttributes.Directory;

    public virtual bool IsDirectoryExists(string path) =>
        Directory.Exists(path) && File.GetAttributes(path) == FileAttributes.Directory;
}