using System.ComponentModel;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DaramRenamer.FileOperators;

namespace DaramRenamer;

[Serializable]
public class FileItem : IComparable<FileItem>, INotifyPropertyChanged
{
    public static ObservableCollection<FileItem> Files { get; set; } = [];
    public static IFileOperator FileOperator { get; set; } = new DefaultFileOperator();

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

    public string SourcePath => GetDirectoryName(SourceFullPath);
    public string SourceName => GetFileName(SourceFullPath);
    public string OriginalFullPath => SourceFullPath;
    public string OriginalPath => SourcePath;
    public string OriginalFilename => SourceName;

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

    public string ChangedFilename
    {
        get => ChangedName;
        set => ChangedName = value;
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

    public void Reset()
    {
        ChangedName = SourceName;
        ChangedPath = SourcePath;
    }

    public void Changed()
    {
        Apply();
    }

    public bool Move(bool overwrite, out ErrorCode errorMessage)
    {
        try
        {
            FileOperator.Move(SourceFullPath, ChangedFullPath, overwrite);
            errorMessage = ErrorCode.NoError;
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            errorMessage = ErrorCode.UnauthorizedAccess;
        }
        catch (PathTooLongException)
        {
            errorMessage = ErrorCode.PathTooLong;
        }
        catch (DirectoryNotFoundException)
        {
            errorMessage = ErrorCode.DirectoryNotFound;
        }
        catch (FileNotFoundException)
        {
            errorMessage = ErrorCode.FileNotFound;
        }
        catch (IOException)
        {
            errorMessage = ErrorCode.IOError;
        }
        catch
        {
            errorMessage = ErrorCode.Unknown;
        }

        return false;
    }

    public bool Copy(bool overwrite, out ErrorCode errorMessage)
    {
        try
        {
            FileOperator.Copy(SourceFullPath, ChangedFullPath, overwrite);
            errorMessage = ErrorCode.NoError;
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            errorMessage = ErrorCode.UnauthorizedAccess;
        }
        catch (PathTooLongException)
        {
            errorMessage = ErrorCode.PathTooLong;
        }
        catch (DirectoryNotFoundException)
        {
            errorMessage = ErrorCode.DirectoryNotFound;
        }
        catch (FileNotFoundException)
        {
            errorMessage = ErrorCode.FileNotFound;
        }
        catch (IOException)
        {
            errorMessage = overwrite ? ErrorCode.FailedOverwrite : ErrorCode.IOError;
        }
        catch
        {
            errorMessage = ErrorCode.Unknown;
        }

        return false;
    }

    public static void Sort(ObservableCollection<FileItem> files)
    {
        var ordered = files.OrderBy(item => item).ToArray();
        files.Clear();
        foreach (var item in ordered)
            files.Add(item);
    }

    public static void Apply(bool autoFix, RenameMode renameMode, bool overwrite,
        Action<FileItem, ErrorCode> progressIncrement)
    {
        var succeededItems = new ConcurrentQueue<FileItem>();
        FileOperator.Begin();

        try
        {
            foreach (var item in Files)
            {
                if (autoFix)
                {
                    item.ChangedPath = ReplaceInvalidPathCharacters(item.ChangedPath);
                    item.ChangedName = ReplaceInvalidFilenameCharacters(item.ChangedName);
                }

                ErrorCode errorCode;
                switch (renameMode)
                {
                    case RenameMode.Move:
                        item.Move(overwrite, out errorCode);
                        break;
                    case RenameMode.Copy:
                        item.Copy(overwrite, out errorCode);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(renameMode), renameMode, null);
                }

                progressIncrement(item, errorCode);
                if (errorCode == ErrorCode.NoError)
                    succeededItems.Enqueue(item);
            }
        }
        finally
        {
            FileOperator.End();
        }

        foreach (var item in succeededItems)
            item.Changed();
    }

    public static string ReplaceInvalidFilenameCharacters(string filename)
    {
        foreach (var ch in Path.GetInvalidFileNameChars())
            filename = filename.Replace(ch, GetInvalidToValid(ch));
        return filename;
    }

    public static string ReplaceInvalidPathCharacters(string path)
    {
        foreach (var ch in Path.GetInvalidPathChars())
            path = path.Replace(ch, GetInvalidToValid(ch));
        return path;
    }

    public static char GetInvalidToValid(char ch) =>
        ch switch
        {
            '?' => '？',
            '\\' => '＼',
            '/' => '／',
            '<' => '〈',
            '>' => '〉',
            '*' => '＊',
            '|' => '｜',
            ':' => '：',
            '"' => '＂',
            '%' => '％',
            '.' => '．',
            '\a' or '\b' or '\t' or '\n' or '\v' or '\f' or '\r' or '\0' => ' ',
            _ when char.IsControl(ch) => ' ',
            _ => ch
        };

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

    private static string GetFileName(string path)
    {
        var index = path.LastIndexOfAny(['\\', '/']);
        return index >= 0 ? path[(index + 1)..] : Path.GetFileName(path);
    }

    private static string GetDirectoryName(string path)
    {
        var index = path.LastIndexOfAny(['\\', '/']);
        return index > 0 ? path[..index] : Path.GetDirectoryName(path) ?? string.Empty;
    }
}
