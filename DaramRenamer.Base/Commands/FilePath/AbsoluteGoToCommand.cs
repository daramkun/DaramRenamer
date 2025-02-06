using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AbsoluteGoTo")]
public class AbsoluteGoToCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Path;
    
    [LocalizationKey("Command_Argument_AbsoluteGoTo_Path")]
    public DirectoryInfo Path { get; set; } = new(Environment.CurrentDirectory);
    
    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (!Path.Exists) return false;
        fileInfo.ChangedPath = Path.FullName;
        return true;
    }
}