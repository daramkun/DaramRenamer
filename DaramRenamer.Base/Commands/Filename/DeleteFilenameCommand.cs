using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_DeleteFilename")]
public class DeleteFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 7;
    
    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        fileInfo.ChangedName = fileInfo.ChangedNameExtension;
        return true;
    }
}