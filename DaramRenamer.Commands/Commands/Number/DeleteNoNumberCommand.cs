using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DaramRenamer.Commands.Number;

[Serializable]
[LocalizationKey("Command_Name_DeleteNoNumber")]
public class DeleteNoNumberCommand : ICommand
{
    private static readonly char[] Spliters =
    {
        ' ', '[', ']', ',', '.', '(', ')', '{', '}', '<', '>', '　',
        '\t', ':', ';', '*', '&', '@', '^', '-', '_', '=', '+', '~'
    };

    [LocalizationKey("Command_Argument_DeleteNoNumber_Wordly")]
    public bool Wordly { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Number;

    public bool DoCommand(FileInfo file)
    {
        if (file.ChangedFilename.Length == 0) return false;

        var sb = new StringBuilder();
        if (!Wordly)
        {
            foreach (var ch in Path.GetFileNameWithoutExtension(file.ChangedFilename)
                         .Where(ch => ch >= '0' && ch <= '9'))
                sb.Append(ch);
            file.ChangedFilename = $"{sb}{Path.GetExtension(file.ChangedFilename)}";
        }
        else
        {
            var split = new List<string>(Path.GetFileNameWithoutExtension(file.ChangedFilename).Split(Spliters));
            foreach (var str in split)
            {
                foreach (var ch in str.Where(ch => ch >= '0' && ch <= '9'))
                    sb.Append(ch);
                sb.Append(' ');
            }

            sb.Remove(sb.Length - 1, 1);
            file.ChangedFilename = $"{sb}{Path.GetExtension(file.ChangedFilename)}";
        }

        return true;
    }
}