using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_DeleteExtension")]
public class DeleteExtensionCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Extension;
    public override int Order => int.MinValue + 2;

    [LocalizationKey("Command_Argument_DeleteExtension_ApplyToDirectory")]
    public bool ApplyToDirectory { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (!ApplyToDirectory && fileInfo.IsDirectory)
            return true;

        fileInfo.ChangedName = fileInfo.ChangedNameWithoutExtension;
        return true;
    }
}