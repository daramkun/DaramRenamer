namespace DaramRenamer;

public interface IFileOperator : IDisposable
{
    void Begin();
    void End();

    void Move(string source, string target, bool overwrite);
    void Copy(string source, string target, bool overwrite);

    IEnumerable<string> EnumerateFiles(string path, bool topDirOnly);
    
    bool IsFileExists(string path);
    bool IsDirectoryExists(string path);
}