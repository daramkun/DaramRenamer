using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DaramRenamer.Commands.Number;

[Serializable]
[LocalizationKey("Command_Name_SameNumberCount")]
public class SameNumberCountCommand : ICommand
{
    [LocalizationKey("Command_Argument_SameNumberCount_Count")]
    public uint Count { get; set; } = 2;

    [LocalizationKey("Command_Argument_SameNumberCount_Position")]
    public Position1 Position { get; set; } = Position1.EndPoint;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Number;

    public bool DoCommand(FileInfo file)
    {
        if (file.ChangedFilename.Length == 0) return false;
        var fn = Path.GetFileNameWithoutExtension(file.ChangedFilename);

        var meetTheNumber = false;
        uint offset = 0, count = 0, size = 0;
        foreach (var ch in Position == Position1.StartPoint ? fn : fn.Reverse())
        {
            if (ch >= '0' && ch <= '9')
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
                    if (Position == Position1.EndPoint)
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

        sb.Append(Path.GetExtension(file.ChangedFilename));

        file.ChangedFilename = sb.ToString();
        return true;
    }
}