using System.Collections.Generic;
using System.IO;
using Daramee.Winston.File;

namespace DaramRenamer;

internal class WindowsNativeFileOperator : IFileOperator
{
    public void BeginBatch()
    {
        Operation.Begin();
    }

    public void EndBatch()
    {
        Operation.End();
    }

    public void Move(string destination, string source, bool overwrite)
    {
        Operation.Move(destination, source, overwrite);
    }

    public void Copy(string destination, string source, bool overwrite)
    {
        Operation.Copy(destination, source, overwrite);
    }

    public IEnumerable<string> GetFiles(string directory, bool topDirectoryOnly)
    {
        return FilesEnumerator.EnumerateFiles(directory, "*.*", topDirectoryOnly);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path) && File.GetAttributes(path) != FileAttributes.Directory;
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path) && File.GetAttributes(path) == FileAttributes.Directory;
    }
}