using System.Text.RegularExpressions;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_DeleteDate")]
public class DeleteDateCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Date;
    public override int Order => int.MinValue + 1;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        foreach (var regex in DateRegexs)
        {
            var proceed = regex.Replace(fileInfo.ChangedName, "");
            if (proceed == fileInfo.ChangedName)
                continue;

            fileInfo.ChangedName = proceed;
            break;
        }

        return true;
    }
    
    private static readonly Regex[] DateRegexs =
    {
        new("[0-9][0-9][0-9][0-9][0-1][0-9][0-3][0-9]"),
        new("[0-9][0-9][0-9][0-9][0-3][0-9][0-1][0-9]"),
        new("[0-9][0-9][0-1][0-9][0-3][0-9]"),
        new("[0-9][0-9][0-3][0-9][0-1][0-9]"),
        new("[0-9]?[0-9]?[0-9][0-9]/[0-1]?[0-9]/[0-3]?[0-9]"),
        new("[0-9]?[0-9]?[0-9][0-9]/[0-3]?[0-9]/[0-1]?[0-9]"),
        new("[0-9][0-9]?[0-9][0-9]-[0-1]?[0-9]-[0-3]?[0-9]"),
        new("[0-9][0-9]?[0-9][0-9]-[0-3]?[0-9]-[0-1]?[0-9]"),
        new("((Sun)|(Mon)|(Tue)|(Wed)|(Thu)|(Fri)|(Sat)), [0-9][0-9] ((Jan)|(Fab)|(Mar)|(Apr)|(May)|(Jun)|(Jul)|(Aug)|(Sep)|(Oct)|(Nov)|(Dec)) [0-9][0-9][0-9][0-9] [0-2][0-9]:[0-6][0-9]:[0-6][0-9] [A-Z][A-Z][A-Z]")
    };
}