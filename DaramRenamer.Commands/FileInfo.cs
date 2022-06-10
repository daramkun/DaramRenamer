using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaramRenamer;

[Serializable]
public class FileInfo : IComparable<FileInfo>, INotifyPropertyChanged
{
    private string _changedPath, _changedFilename;

    private string _originalFullPath;

    public FileInfo(string fullPath, bool directoryCheck = true)
    {
        OriginalFullPath = fullPath;
        ChangedFilename = OriginalFilename;
        ChangedPath = OriginalPath;
        IsDirectory = directoryCheck && File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory);
    }

    public FileInfo(FileInfo file)
    {
        OriginalFullPath = file.OriginalFullPath;
        ChangedPath = file.ChangedPath;
        ChangedFilename = file.ChangedFilename;
        IsDirectory = file.IsDirectory;
    }

    public FileInfo(string originalFullPath, string changedFilename, string changedPath = null)
    {
        OriginalFullPath = originalFullPath;
        ChangedFilename = changedFilename;
        ChangedPath = changedPath ?? OriginalPath;
        IsDirectory = File.GetAttributes(originalFullPath).HasFlag(FileAttributes.Directory);
    }

    public static ObservableCollection<FileInfo> Files { get; set; } = new();
    public static IFileOperator FileOperator { get; set; } = new DefaultFileOperator();

    public string OriginalFullPath
    {
        get => _originalFullPath;
        private set
        {
            _originalFullPath = value;
            DoPropertyChanged(nameof(OriginalFullPath));
            DoPropertyChanged(nameof(OriginalPath));
            DoPropertyChanged(nameof(OriginalFilename));
        }
    }

    public string OriginalPath => Path.GetDirectoryName(OriginalFullPath);
    public string OriginalFilename => Path.GetFileName(OriginalFullPath);

    public string ChangedPath
    {
        get => _changedPath;
        set
        {
            _changedPath = value;
            DoPropertyChanged(nameof(ChangedPath));
        }
    }

    public string ChangedFilename
    {
        get => _changedFilename;
        set
        {
            _changedFilename = value;
            DoPropertyChanged(nameof(ChangedFilename));
        }
    }

    public string ChangedFullPath => Path.Combine(ChangedPath, ChangedFilename);
    public bool IsDirectory { get; set; }

    public int CompareTo(FileInfo other)
    {
        return string.Compare(ChangedFilename, other.ChangedFilename, StringComparison.Ordinal);
    }

    [field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;

    public static void Sort(ObservableCollection<FileInfo> files)
    {
        if (files == null) return;
        DaramRenamer.Sort.Quicksort(files);
    }

    public static void Apply(bool autoFix, RenameMode renameMode, bool overwrite,
        Action<FileInfo, ErrorCode> progressIncrement)
    {
        var parallelApply = true;
        Parallel.ForEach(Files, (fileInfo, parallelLoopState) =>
        {
            if (!File.Exists(fileInfo.OriginalFullPath)
                || fileInfo.OriginalFullPath == fileInfo.ChangedFullPath) return;
            parallelApply = false;
            parallelLoopState.Break();
        });

        var failed = 0;
        var succeededItems = new ConcurrentQueue<FileInfo>();

        void ItemChanger(FileInfo fileInfo)
        {
            if (autoFix)
            {
                fileInfo.ChangedPath = ReplaceInvalidPathCharacters(fileInfo.ChangedPath);
                fileInfo.ChangedFilename = ReplaceInvalidFilenameCharacters(fileInfo.ChangedFilename);
            }

            ErrorCode errorMessage;
            switch (renameMode)
            {
                case RenameMode.Move:
                    fileInfo.Move(overwrite, out errorMessage);
                    break;
                case RenameMode.Copy:
                    fileInfo.Copy(overwrite, out errorMessage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(renameMode), renameMode, null);
            }

            progressIncrement?.Invoke(fileInfo, errorMessage);

            if (errorMessage != ErrorCode.NoError)
                Interlocked.Increment(ref failed);
            else succeededItems.Enqueue(fileInfo);
        }

        if (parallelApply)
        {
            FileOperator.BeginBatch();
            Parallel.ForEach(Files, ItemChanger);
            FileOperator.EndBatch();
        }
        else
        {
            FileOperator.BeginBatch();
            Parallel.ForEach(from f in Files where !File.Exists(f.ChangedFullPath) select f, ItemChanger);
            FileOperator.EndBatch();

            var sortingFileInfo = new List<FileInfo>(from f in Files
                where !succeededItems.Contains(f) && f.OriginalFullPath != f.ChangedFullPath
                select f);
            var temp = new List<FileInfo>();
            bool changed;
            do
            {
                changed = false;
                FileOperator.BeginBatch();
                foreach (var fileInfo in sortingFileInfo)
                    if (!File.Exists(fileInfo.ChangedFullPath))
                    {
                        ItemChanger(fileInfo);
                        changed = true;
                        temp.Add(fileInfo);
                    }
                    else
                    {
                        if (!succeededItems.Concat(temp).Any(succeededFileInfo =>
                                succeededFileInfo.ChangedFullPath != fileInfo.ChangedFullPath &&
                                succeededFileInfo.OriginalFullPath == fileInfo.ChangedFullPath))
                            continue;
                        ItemChanger(fileInfo);
                        changed = true;
                        temp.Add(fileInfo);
                    }

                FileOperator.EndBatch();

                foreach (var proceed in temp)
                    sortingFileInfo.Remove(proceed);
                temp.Clear();
            } while (changed);

            if (overwrite)
            {
                Parallel.ForEach(sortingFileInfo, ItemChanger);
            }
            else
            {
                failed += sortingFileInfo.Count;
                foreach (var fileInfo in sortingFileInfo)
                    progressIncrement(fileInfo, ErrorCode.IOError);
            }
        }

        Parallel.ForEach(succeededItems, fileInfo => fileInfo.Changed());
    }

    public bool Move(bool overwrite, out ErrorCode errorMessage)
    {
        try
        {
            FileOperator.Move(ChangedFullPath, OriginalFullPath, overwrite);
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
        catch (Exception)
        {
            errorMessage = ErrorCode.Unknown;
        }

        return false;
    }

    public bool Copy(bool overwrite, out ErrorCode errorMessage)
    {
        try
        {
            FileOperator.Copy(ChangedFullPath, OriginalFullPath, overwrite);
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
        catch (Exception)
        {
            errorMessage = ErrorCode.Unknown;
        }

        return false;
    }

    public void Reset()
    {
        ChangedFilename = OriginalFilename;
        ChangedPath = OriginalPath;
    }

    public void Changed()
    {
        OriginalFullPath = ChangedFullPath;
    }

    public override bool Equals(object obj)
    {
        return obj is FileInfo info
               && OriginalFullPath == info.OriginalFullPath;
    }

    public override int GetHashCode()
    {
        return OriginalFullPath.GetHashCode();
    }

    public override string ToString()
    {
        return
            $"{{ChangedFilename: {ChangedFilename}, ChangedPath: {ChangedPath}, OriginalFullPath: {OriginalFullPath}}}";
    }

    private void DoPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #region Filename Helpers

    public static char GetInvalidToValid(char ch)
    {
        switch (ch)
        {
            case '?': return '？';
            case '\\': return '＼';
            case '/': return '／';
            case '<': return '〈';
            case '>': return '〉';
            case '*': return '＊';
            case '|': return '｜';
            case ':': return '：';
            case '"': return '＂';
            case '%': return '％';
            case '.': return '．';
            case '\a':
            case '\b':
            case '\t':
            case '\n':
            case '\v':
            case '\f':
            case '\r':
            case '\0':
            case '\u0001':
            case '\u0002':
            case '\u0003':
            case '\u0004':
            case '\u0005':
            case '\u0006':
            case '\u000e':
            case '\u000f':
            case '\u0010':
            case '\u0011':
            case '\u0012':
            case '\u0013':
            case '\u0014':
            case '\u0015':
            case '\u0016':
            case '\u0017':
            case '\u0018':
            case '\u0019':
            case '\u001a':
            case '\u001b':
            case '\u001c':
            case '\u001d':
            case '\u001e':
            case '\u001f':
                return ' ';
            default: return ch;
        }
    }

    public static string ReplaceInvalidPathCharacters(string path)
    {
        StringBuilder builder = null;
        foreach (var ch in Path.GetInvalidPathChars())
        {
            if (path.IndexOf(ch) < 0)
                continue;

            builder ??= new StringBuilder(path);

            builder.Replace(ch, GetInvalidToValid(ch));
        }

        return builder?.ToString() ?? path;
    }

    public static string ReplaceInvalidFilenameCharacters(string filename)
    {
        StringBuilder builder = null;
        foreach (var ch in Path.GetInvalidFileNameChars())
        {
            if (filename.IndexOf(ch) < 0)
                continue;

            builder ??= new StringBuilder(filename);

            builder.Replace(ch, GetInvalidToValid(ch));
        }

        return builder?.ToString() ?? filename;
    }

    #endregion
}