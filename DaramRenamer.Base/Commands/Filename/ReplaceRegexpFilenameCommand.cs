using System.Text.RegularExpressions;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_ReplaceRegexp")]
public class ReplaceRegexpFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 1;
    
    [LocalizationKey("Command_Argument_ReplaceRegexp_Find")]
    public string Find { get; set; } = "$^";

    [LocalizationKey("Command_Argument_ReplaceRegexp_Replace")]
    public string Replace { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_ReplaceRegexp_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var filename = IncludeExtension ? fileInfo.ChangedName : fileInfo.ChangedNameWithoutExtension;
        var ext = IncludeExtension ? string.Empty : fileInfo.ChangedNameExtension;

        fileInfo.ChangedName = $"{Regex.Replace(filename, Find, Replace)}{ext}";

        return true;
    }
}