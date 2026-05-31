using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DaramRenamer;

[Serializable]
public class FileItem : IComparable<FileItem>, INotifyPropertyChanged
{
    public string SourceFullPath
    {
        get;
        set
        {
            field = value;
            DoPropertyChanged();
            DoPropertyChanged(nameof(SourcePath));
            DoPropertyChanged(nameof(SourceName));
        }
    }

    public string SourcePath => Path.GetDirectoryName(SourceFullPath) ?? string.Empty;
    public string SourceName => Path.GetFileName(SourceFullPath);

    public string ChangedFullPath => Path.Combine(ChangedPath, ChangedName);

    public string ChangedPath
    {
        get;
        set
        {
            field = value;
            DoPropertyChanged();
            DoPropertyChanged(nameof(ChangedFullPath));
        }
    }

    public string ChangedName
    {
        get;
        set
        {
            field = value;
            DoPropertyChanged();
            DoPropertyChanged(nameof(ChangedFullPath));
            DoPropertyChanged(nameof(ChangedNameWithoutExtension));
            DoPropertyChanged(nameof(ChangedExtension));
        }
    }

    public bool IsDirectory
    {
        get;
        set
        {
            field = value;
            DoPropertyChanged();
        }
    }
    
    public string ChangedNameWithoutExtension => Path.GetFileNameWithoutExtension(ChangedName);
    public string ChangedExtension => Path.GetExtension(ChangedName);

    public FileItem(string path, bool checkIsDirectory = true)
    {
        SourceFullPath = path;
        ChangedName = SourceName;
        ChangedPath = SourcePath;
        IsDirectory = checkIsDirectory && File.GetAttributes(path).HasFlag(FileAttributes.Directory);
    }

    public FileItem(FileItem item)
    {
        SourceFullPath = item.SourceFullPath;
        ChangedName = item.ChangedName;
        ChangedPath = item.ChangedPath;
        IsDirectory = item.IsDirectory;
    }

    public FileItem(string sourceFullPath, string changedName, string changedPath, bool isDirectory)
    {
        SourceFullPath = sourceFullPath;
        ChangedName = changedName;
        ChangedPath = changedPath;
        IsDirectory = isDirectory;
    }

    public void Apply()
    {
        SourceFullPath = Path.Combine(ChangedPath, ChangedName);
    }

    public override bool Equals(object? obj) => obj is FileItem item && SourceFullPath == item.SourceFullPath;

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() => SourceFullPath.GetHashCode();

    public int CompareTo(FileItem? other)
    {
        if (other == null)
            return 1;
        
        if (IsDirectory != other.IsDirectory)
            return IsDirectory ? 1 : -1;
        
        return string.Compare(ChangedName, other.ChangedName, StringComparison.Ordinal);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void DoPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public override string ToString() => $"{{Source: \"{SourceFullPath}\", Changed: \"{ChangedFullPath}\"}}";
}