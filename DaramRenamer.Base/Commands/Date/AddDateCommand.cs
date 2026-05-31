using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class AddDateCommand : ICommand
{
    public FileDateKind Kind
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Kind)));
        }
    } = FileDateKind.Creation;

    public string Format
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Format)));
        }
    } = "yyMMdd";

    public Position3 Position
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
        }
    } = Position3.End;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 1;
    public CommandCategory Category => CommandCategory.Date;

    public void Apply(FileItem item, int _)
    {
        var name = item.ChangedNameWithoutExtension;
        var ext = item.ChangedExtension;

        var date = Kind switch
        {
            FileDateKind.Creation => File.GetCreationTime(item.SourceFullPath).ToString(Format),
            FileDateKind.LastModify => File.GetLastAccessTime(item.SourceFullPath).ToString(Format),
            FileDateKind.LastAccess => File.GetLastWriteTime(item.SourceFullPath).ToString(Format),
            FileDateKind.Now => DateTime.Now.ToString(Format),
            _ => throw new ArgumentOutOfRangeException(nameof(Kind))
        };

        item.ChangedName = Position switch
        {
            Position3.Begin => $"{date}{name}{ext}",
            Position3.End => $"{name}{date}{ext}",
            Position3.Both => $"{date}{name}{date}{ext}",
            _ => throw new ArgumentOutOfRangeException(nameof(Position))
        };
    }
}