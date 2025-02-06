using System.Text.RegularExpressions;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_RearrangeRegexp")]
public class RearrangeRegexpFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 2;
    
    [LocalizationKey("Command_Argument_RearrangeRegexp_Find")]
    public string Regexp { get; set; } = "$^";

    [LocalizationKey("Command_Argument_RearrangeRegexp_Replace")]
    public string FormatString { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_RearrangeRegexp_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var filename = IncludeExtension ? fileInfo.ChangedName : fileInfo.ChangedNameWithoutExtension;
        var ext = IncludeExtension ? string.Empty : fileInfo.ChangedNameExtension;
        
        var match = Regex.Match(filename, Regexp);
        
        try
        {
            var group = match.Groups;
            var groupArr = new object[group.Count];
            for (var i = 0; i < groupArr.Length; i++)
                groupArr[i] = group[i].Value.Trim();
            
            fileInfo.ChangedName = $"{string.Format(FormatString, groupArr)}{ext}";
        }
        catch
        {
            return false;
        }

        return true;
    }
}