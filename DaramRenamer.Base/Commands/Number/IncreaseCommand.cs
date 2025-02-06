using System.Text;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_Increase")]
public class IncreaseCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Number;
    
    [LocalizationKey("Command_Argument_Increase_Count")]
    public int Count { get; set; } = 1;

    [LocalizationKey("Command_Argument_Increase_Position")]
    public Direction Direction { get; set; } = Direction.BottomUp;

    public override bool DoCommand(BaseFileInfo file)
    {
        if (Count == 0) return true;
        if (file.ChangedName.Length == 0) return false;
        var fn = file.ChangedNameWithoutExtension;

        var meetTheNumber = false;
        int offset = 0, count = 0, size = 0;
        foreach (var ch in Direction switch
                 {
                     Direction.BottomUp => fn.Reverse(),
                     Direction.TopDown => fn,
                     _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
                 })
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

        if (!meetTheNumber) return false;

        if (Direction == Direction.BottomUp)
            offset = fn.Length - (offset + size);

        var origin = fn.Substring(offset, size);
        var number = int.Parse(origin) + Count;

        var sb = new StringBuilder();
        sb.Append(number.ToString().PadLeft(size, '0'));
        fn = fn.Remove(offset, size).Insert(offset, sb.ToString());

        file.ChangedName = fn + file.ChangedNameExtension;
        return true;
    }
}