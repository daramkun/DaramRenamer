using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class ConcatDirectoryCommand : ICommand
{
    public Position3 Position { get; set; } = Position3.Begin;

    public bool ApplyToDirectory { get; set; } = false;

    public bool IncludeExtension { get; set; } = false;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 5;
    public CommandCategory Category => CommandCategory.Filename;

    public void Apply(FileItem item, int _)
    {
        if (!ApplyToDirectory && item.IsDirectory)
            return;

        var filename =
            !IncludeExtension
                ? item.ChangedNameWithoutExtension
                : item.ChangedName;
        var ext =
            !IncludeExtension
                ? item.ChangedExtension
                : string.Empty;

        var startIndex = item.ChangedPath.LastIndexOf('\\');
        if (startIndex < 0)
            startIndex = item.ChangedPath.LastIndexOf('/');

        var text =
            startIndex >= 0
                ? item.ChangedPath[(startIndex + 1)..]
                : string.Empty;

        item.ChangedName = Position switch
        {
            Position3.Begin => $"{text}{filename}{ext}",
            Position3.End => $"{filename}{text}{ext}",
            Position3.Both => $"{text}{filename}{text}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}