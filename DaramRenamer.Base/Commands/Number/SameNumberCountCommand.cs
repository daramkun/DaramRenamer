using System.ComponentModel;
using Cysharp.Text;

namespace DaramRenamer.Commands;

[Serializable]
public class SameNumberCountCommand : ICommand
{
    public uint Count
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    } = 2;

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
    
    public int Order => int.MinValue + 4;
    public CommandCategory Category => CommandCategory.Number;
    
    public void Apply(FileItem item, int index)
    {
        if (item.ChangedName.Length == 0)
            return;

        var fn = item.ChangedNameWithoutExtension;

        var meetTheNumber = false;
        uint offset = 0, count = 0, size = 0;
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
                {
                    if (Position == Position2.End)
                        offset = (uint) fn.Length - (offset + size);
                    break;
                }
            }

            ++count;
        }

        if (!meetTheNumber || size >= Count)
            return;

        var sb = ZString.CreateStringBuilder();
        sb.Append(fn);
        size = Count - size;
        while (size > 0)
        {
            sb.Insert((int)offset, "0");
            --size;
        }

        sb.Append(item.ChangedExtension);

        item.ChangedName = sb.ToString();
    }
}