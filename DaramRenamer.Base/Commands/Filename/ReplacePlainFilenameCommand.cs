using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_ReplacePlain")]
public class ReplacePlainFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue;

    [LocalizationKey("Command_Argument_ReplacePlain_Find")]
    public string Find { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_ReplacePlain_Replace")]
    public string Replace { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_ReplacePlain_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;
    
    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (string.IsNullOrEmpty(Find))
            return false;
        
        Replace ??= string.Empty;
        
        var filename = IncludeExtension ? fileInfo.ChangedName : fileInfo.ChangedNameWithoutExtension;
        var ext = IncludeExtension ? string.Empty : fileInfo.ChangedNameExtension;

        fileInfo.ChangedName = $"{filename.Replace(Find, Replace)}{ext}";

        return true;
    }
}