using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_ReplaceExtension")]
public class ReplaceExtensionCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Extension;
    public override int Order => int.MinValue + 3;
    
    [LocalizationKey("Command_Argument_ReplaceExtension_Extension")]
    public string Extension { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_ReplaceExtension_ApplyToDirectory")]
    public bool ApplyToDirectory { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (string.IsNullOrEmpty(Extension))
            return false;

        if (!ApplyToDirectory && fileInfo.IsDirectory)
            return true;

        fileInfo.ChangedName = $"{fileInfo.ChangedNameWithoutExtension}{(Extension[0] != '.' ? "." : string.Empty)}{Extension}";
        
        return true;
    }
}