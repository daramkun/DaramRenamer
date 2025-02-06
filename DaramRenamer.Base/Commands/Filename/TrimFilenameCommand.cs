using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_Trim")]
public class TrimFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 4;
    
    [LocalizationKey("Command_Argument_Trim_Position")]
    public Position Position { get; set; } = Position.Both;
    
    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var filename = fileInfo.ChangedNameWithoutExtension;
        var ext = fileInfo.ChangedNameExtension;

        fileInfo.ChangedName = Position switch
        {
            Position.Begin => $"{filename.TrimStart()}{ext}",
            Position.End => $"{filename.TrimEnd()}{ext}",
            Position.Both => $"{filename.Trim()}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };

        return true;
    }
}