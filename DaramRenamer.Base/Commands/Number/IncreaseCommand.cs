using System.ComponentModel;
using Cysharp.Text;

namespace DaramRenamer.Commands;

[Serializable]
public class IncreaseCommand : ICommand
{
    public int Count
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    } = 1;

    public Position2 Position
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
        }
    } = Position2.End;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 3;
    public CommandCategory Category => CommandCategory.Number;
    
    public void Apply(FileItem item, int _)
    {
        if (Count == 0 || item.ChangedName.Length == 0)
            return;
        
        var fn = item.ChangedNameWithoutExtension;

        var meetTheNumber = false;
        int offset = 0, count = 0, size = 0;
        foreach (var ch in Position == Position2.Begin ? fn : fn.Reverse())
        {
            if (ch is >= '0' and <= '9')
            {
                if (!meetTheNumber)
                {
                    offset = count;
                    meetTheNumber = true;
                }

                ++size;
            }
            else
            {
                if (meetTheNumber)
                    break;
            }

            ++count;
        }

        if (!meetTheNumber)
            return;

        if (Position == Position2.End)
            offset = fn.Length - (offset + size);

        var origin = fn.Substring(offset, size);
        var number = int.Parse(origin) + Count;

        var sb = ZString.CreateStringBuilder();
        sb.Append(number.ToString().PadLeft(size, '0'));
        fn = fn.Remove(offset, size).Insert(offset, sb.ToString());

        item.ChangedName = $"{fn}{item.ChangedExtension}";
    }
}