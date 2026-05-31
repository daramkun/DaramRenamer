using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class ConcatCommand : ICommand
{
    public string Text
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }
    } = string.Empty;

    public Position3 Position
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
        }
    } = Position3.Begin;

    public bool IncludeExtension { get; set; } = false;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 4;
    public CommandCategory Category => CommandCategory.Filename;

    public void Apply(FileItem item, int _)
    {
        if (string.IsNullOrEmpty(Text))
            return;

        var name =
            !IncludeExtension
                ? item.ChangedNameWithoutExtension
                : item.ChangedName;
        var ext =
            !IncludeExtension
                ? item.ChangedExtension
                : string.Empty;

        item.ChangedName = Position switch
        {
            Position3.Begin => $"{Text}{name}{ext}",
            Position3.End => $"{name}{Text}{ext}",
            Position3.Both => $"{Text}{name}{Text}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}