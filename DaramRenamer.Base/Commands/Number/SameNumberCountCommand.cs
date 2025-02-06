using System.Text;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_SameNumberCount")]
public class SameNumberCountCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Number;
    
    [LocalizationKey("Command_Argument_SameNumberCount_Count")]
    public uint Count { get; set; } = 2;

    [LocalizationKey("Command_Argument_SameNumberCount_Position")]
    public Direction Direction { get; set; } = Direction.BottomUp;

    public override bool DoCommand(BaseFileInfo file)
    {
        if (file.ChangedName.Length == 0) return false;
        var fn = Path.GetFileNameWithoutExtension(file.ChangedName);

        var meetTheNumber = false;
        uint offset = 0, count = 0, size = 0;
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
                {
                    if (Direction == Direction.BottomUp)
                        offset = (uint) fn.Length - (offset + size);
                    break;
                }
            }

            ++count;
        }

        if (!meetTheNumber || size >= Count) return false;

        var sb = new StringBuilder();
        sb.Append(fn);
        size = Count - size;
        while (size > 0)
        {
            sb.Insert((int) offset, '0');
            --size;
        }

        sb.Append(Path.GetExtension(file.ChangedName));

        file.ChangedName = sb.ToString();
        return true;
    }
}