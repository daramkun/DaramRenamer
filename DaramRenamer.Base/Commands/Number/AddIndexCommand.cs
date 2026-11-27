using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class AddIndexCommand : ICommand, ITargetContains
{
    [NonSerialized] private int _targetCount;

    public Position3 Position { get; set; } = Position3.End;
    public bool PadZero { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 1;
    public CommandCategory Category => CommandCategory.Number;

    public void SetTargets(IEnumerable<FileItem> targets) => _targetCount = targets.Count();

    public void Apply(FileItem item, int index)
    {
        var number = (index + 1).ToString();
        if (PadZero)
        {
            var count = Math.Max(_targetCount > 0 ? _targetCount : FileItem.Files.Count, index + 1);
            number = number.PadLeft(count.ToString().Length, '0');
        }

        item.ChangedName = string.Format(Position switch
            {
                Position3.Begin => "{1}{0}{2}",
                Position3.End => "{0}{1}{2}",
                Position3.Both => "{1}{0}{1}{2}",
                _ => throw new ArgumentOutOfRangeException()
            },
            item.ChangedNameWithoutExtension, number, item.ChangedExtension);
    }
}
