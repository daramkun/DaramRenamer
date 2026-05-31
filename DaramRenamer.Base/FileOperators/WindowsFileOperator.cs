using System.Runtime.Versioning;
using DaramRenamer.Helpers;

namespace DaramRenamer.FileOperators;

[SupportedOSPlatform("windows")]
public partial class WindowsFileOperator : DefaultFileOperator
{
    private readonly WindowsNativeFileOperation _fileOperation = new();

    protected override void Dispose(bool disposing)
    {
        _fileOperation.Dispose();
    }

    public override void Begin()
    {
        _fileOperation.SetOperationFlags(WindowsNativeFileOperation.OperationFlags.AllowUndo |
                                         WindowsNativeFileOperation.OperationFlags.NoErrorUI);
    }

    public override void End()
    {
        try
        {
            _fileOperation.PerformOperations();
        }
        catch (Exception ex)
        {
            if (!ex.HResult.Equals(unchecked((int)0x8000FFFF)))
                throw;
        }
    }

    public override void Move(string source, string target, bool overwrite)
    {
        if (Path.GetFullPath(source).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
            return;

        if (!overwrite)
            target = GetNonOverwriteFileName(target);

        var sourceDir = Path.GetDirectoryName(source) ?? string.Empty;
        var targetDir = Path.GetDirectoryName(target) ?? string.Empty;
        var targetName = Path.GetFileName(target);
        if (!sourceDir.Equals(targetDir, StringComparison.OrdinalIgnoreCase))
        {
            _fileOperation.MoveItem(source, targetDir, targetName);
        }
        else
        {
            _fileOperation.RenameItem(source, targetName);
        }
    }

    public override void Copy(string source, string target, bool overwrite)
    {
        if (Path.GetFullPath(source).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
            return;
        
        if (!overwrite)
            target = GetNonOverwriteFileName(target);

        var targetDir = Path.GetDirectoryName(target) ?? string.Empty;
        var targetName = Path.GetFileName(target);
        _fileOperation.CopyItem(source, targetDir, targetName);
    }

    private static string GetNonOverwriteFileName(string source)
    {
        if (!File.Exists(source))
            return source;

        uint count = 1;
        var path = Path.GetDirectoryName(source) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);

        while (count < 0x7fffffff)
        {
            var newFilename = Path.Combine(path, $"{name} ({count}){ext}");
            if (!File.Exists(newFilename))
                return newFilename;

            ++count;
        }

        throw new IOException();
    }
}