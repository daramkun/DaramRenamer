using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AddDate")]
public class AddDateCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Date;
    public override int Order => int.MinValue;
    
    [LocalizationKey("Command_Argument_AddDate_Kind")]
    public FileDateKind Kind { get; set; } = FileDateKind.Creation;

    [LocalizationKey("Command_Argument_AddDate_Format")]
    public string Format { get; set; } = "yyMMdd";

    [LocalizationKey("Command_Argument_AddDate_Position")]
    public Position Position { get; set; } = Position.End;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var filename = fileInfo.ChangedNameWithoutExtension;
        var ext = fileInfo.ChangedNameExtension;

        var date = (Kind switch
        {
            FileDateKind.Creation => File.GetCreationTime(fileInfo.OriginalFullPath),
            FileDateKind.LastAccess => File.GetLastAccessTime(fileInfo.OriginalFullPath),
            FileDateKind.LastModify => File.GetLastWriteTime(fileInfo.OriginalFullPath),
            FileDateKind.Now => DateTime.Now,
            _ => throw new ArgumentOutOfRangeException()
        }).ToString(Format);

        fileInfo.ChangedName = Position switch
        {
            Position.Begin => $"{date}{filename}{ext}",
            Position.End => $"{filename}{date}{ext}",
            Position.Both => $"{date}{filename}{date}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return true;
    }
}