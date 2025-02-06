using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DaramRenamer;

[Serializable]
public abstract class BaseFileInfo(BaseFileSystem fileSystem, string originalFullPath, bool isDirectory) : INotifyPropertyChanged, IComparable<BaseFileInfo>, ICloneable
{
    private string _originalFullPath = originalFullPath;

    private string
        _changedPath = Path.GetDirectoryName(originalFullPath) ?? string.Empty,
        _changedName = Path.GetFileName(originalFullPath);

    private bool _isDirectory = isDirectory;

    public BaseFileSystem BaseFileSystem { get; } = fileSystem;

    public string OriginalPath => Path.GetDirectoryName(_originalFullPath) ?? string.Empty;
    public string OriginalName => Path.GetFileName(_originalFullPath);
    public string OriginalFullPath => _originalFullPath;

    public string ChangedPath
    {
        get => _changedPath;
        set
        {
            _changedPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ChangedFullPath));
        }
    }

    public string ChangedName
    {
        get => _changedName;
        set
        {
            _changedName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ChangedFullPath));
        }
    }

    public string ChangedFullPath => Path.Combine(_changedPath, _changedName);
    
    public string ChangedNameWithoutExtension => Path.GetFileNameWithoutExtension(_changedName);
    public string ChangedNameExtension => Path.GetExtension(_changedName);

    public bool IsDirectory => _isDirectory;

    protected abstract bool TryApply(BaseFileOperator fileOperator, FileInfoApplyOptions options);

    public void Apply(BaseFileOperator fileOperator, FileInfoApplyOptions options)
    {
        if (!TryApply(fileOperator, options))
            return;
        
        _originalFullPath = Path.Combine(_changedPath, _changedName);
        OnPropertyChanged(nameof(OriginalPath));
        OnPropertyChanged(nameof(OriginalName));
    }

    public void Reset()
    {
        ChangedPath = OriginalPath;
        ChangedName = OriginalName;
    }
    
    [field: NonSerialized]
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public int CompareTo(BaseFileInfo? other) => other != null
        ? string.Compare(ChangedName, other.ChangedName, StringComparison.OrdinalIgnoreCase)
        : 1;

    public abstract object Clone();
}