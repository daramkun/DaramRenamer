using System.Text;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_DeleteNoNumber")]
public class DeleteNoNumberCommand : BaseCommand
{
    private static readonly char[] Spliters =
    {
        ' ', '[', ']', ',', '.', '(', ')', '{', '}', '<', '>', '　',
        '\t', ':', ';', '*', '&', '@', '^', '-', '_', '=', '+', '~'
    };
    
    public override CommandCategory Category => CommandCategory.Number;

    [LocalizationKey("Command_Argument_DeleteNoNumber_Wordly")]
    public bool Wordly { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (fileInfo.ChangedName.Length == 0) return false;

        var sb = new StringBuilder();
        if (!Wordly)
        {
            foreach (var ch in fileInfo.ChangedNameWithoutExtension
                         .Where(ch => ch is >= '0' and <= '9'))
                sb.Append(ch);
            fileInfo.ChangedName = $"{sb}{fileInfo.ChangedNameExtension}";
        }
        else
        {
            var split = new List<string>(fileInfo.ChangedNameWithoutExtension.Split(Spliters));
            foreach (var str in split)
            {
                foreach (var ch in str.Where(ch => ch >= '0' && ch <= '9'))
                    sb.Append(ch);
                sb.Append(' ');
            }

            sb.Remove(sb.Length - 1, 1);
            fileInfo.ChangedName = $"{sb}{fileInfo.ChangedNameExtension}";
        }

        return true;
    }
}