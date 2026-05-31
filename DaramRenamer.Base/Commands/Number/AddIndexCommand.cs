using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class AddIndexCommand : ICommand
{
    public Position3 Position { get; set; } = Position3.End;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 1;
    public CommandCategory Category => CommandCategory.Number;

    public void Apply(FileItem item, int index)
    {
        item.ChangedName = string.Format(Position switch
            {
                Position3.Begin => "{1}{0}{2}",
                Position3.End => "{0}{1}{2}",
                Position3.Both => "{1}{0}{1}{2}",
                _ => throw new ArgumentOutOfRangeException()
            },
            item.ChangedNameWithoutExtension, index + 1, item.ChangedExtension);
    }
}