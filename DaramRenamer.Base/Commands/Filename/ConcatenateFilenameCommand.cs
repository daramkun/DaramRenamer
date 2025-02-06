using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_Concatenate")]
public class ConcatenateFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 3;
    
    [LocalizationKey("Command_Argument_Concatenate_Text")]
    public string Text { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_Concatenate_Position")]
    public Position Position { get; set; } = Position.Begin;

    [LocalizationKey("Command_Argument_Concatenate_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;
    
    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (string.IsNullOrEmpty(Text))
            return false;

        var filename = IncludeExtension ? fileInfo.ChangedName : fileInfo.ChangedNameWithoutExtension;
        var ext = IncludeExtension ? string.Empty : fileInfo.ChangedNameExtension;

        fileInfo.ChangedName = Position switch
        {
            Position.Begin => $"{Text}{filename}{ext}",
            Position.End => $"{filename}{Text}{ext}",
            Position.Both => $"{Text}{filename}{Text}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };

        return true;
    }
}