using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AddExtension")]
public class AddExtensionCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Extension;
    public override int Order => int.MinValue;
    
    [LocalizationKey("Command_Argument_AddExtension_Extension")]
    public string Extension { get; set; } = string.Empty;

    [LocalizationKey("Commamd_Argument_AddExtension_ApplyToDirectory")]
    public bool ApplyToDirectory { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (string.IsNullOrEmpty(Extension))
            return false;

        if (!ApplyToDirectory && fileInfo.IsDirectory)
            return true;

        fileInfo.ChangedName = $"{fileInfo.ChangedName}{(Extension[0] != '.' ? "." : "")}{Extension}";
        return true;
    }
}