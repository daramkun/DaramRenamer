using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DaramRenamer.Commands.Number;

[Serializable]
[LocalizationKey("Command_Name_Increase")]
public class IncreaseCommand : ICommand
{
    [LocalizationKey("Command_Argument_Increase_Count")]
    public int Count { get; set; } = 1;

    [LocalizationKey("Command_Argument_Increase_Position")]
    public Position1 Position { get; set; } = Position1.EndPoint;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Number;

    public bool DoCommand(FileInfo file)
    {
        if (Count == 0) return true;
        if (file.ChangedFilename.Length == 0) return false;
        var fn = Path.GetFileNameWithoutExtension(file.ChangedFilename);

        var meetTheNumber = false;
        int offset = 0, count = 0, size = 0;
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
                    break;
            }

            ++count;
        }

        if (!meetTheNumber) return false;

        if (Position == Position1.EndPoint)
            offset = fn.Length - (offset + size);

        var origin = fn.Substring(offset, size);
        var number = int.Parse(origin) + Count;

        var sb = new StringBuilder();
        sb.Append(number.ToString().PadLeft(size, '0'));
        fn = fn.Remove(offset, size).Insert(offset, sb.ToString());

        file.ChangedFilename = fn + Path.GetExtension(file.ChangedFilename);
        return true;
    }
}