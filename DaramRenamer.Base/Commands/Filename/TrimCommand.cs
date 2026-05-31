using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class TrimCommand : ICommand
{
    public Position3 Position
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
        }
    } = Position3.Both;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 6;
    public CommandCategory Category => CommandCategory.Filename;

    public void Apply(FileItem item, int _)
    {
        var filename = item.ChangedNameWithoutExtension;
        filename = Position switch
        {
            Position3.Begin => filename.TrimStart(),
            Position3.End => filename.TrimEnd(),
            Position3.Both => filename.Trim(),
            _ => throw new ArgumentOutOfRangeException()
        };
        item.ChangedName = $"{filename}{item.ChangedExtension}";
    }
}