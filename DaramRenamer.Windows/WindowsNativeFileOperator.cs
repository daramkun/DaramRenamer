using System.Collections.Generic;
using System.IO;
using DaramRenamer.FileOperators;

namespace DaramRenamer;

internal sealed class WindowsNativeFileOperator : DefaultFileOperator
{
    public override IEnumerable<string> EnumerateFiles(string path, bool topDirOnly)
    {
        if (!Directory.Exists(path))
            return [];

        return Directory.EnumerateFiles(path, "*.*",
            topDirOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
    }
}
