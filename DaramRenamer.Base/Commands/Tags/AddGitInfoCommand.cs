using DaramRenamer.Attributes;
using DaramRenamer.Utilities;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AddGitInfo")]
public class AddGitInfoCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Tag;
    
    [LocalizationKey("Command_Argument_AddGitInfo_GitInfo")]
    public GitDetermineKind GitInfo { get; set; } = GitDetermineKind.CommitId;

    [LocalizationKey("Command_Argument_AddGitInfo_Position")]
    public Position Position { get; set; } = Position.End;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var value = GitUtil.Determine(GitInfo, fileInfo.OriginalFullPath);
        if (value == null)
            return false;

        var fn = fileInfo.ChangedNameWithoutExtension;
        var ext = fileInfo.ChangedNameExtension;
        fileInfo.ChangedName = Position switch
        {
            Position.Begin => $"{value}{fn}{ext}",
            Position.End => $"{fn}{value}{ext}",
            Position.Both => $"{value}{fn}{value}{ext}", 
            _ => $"{value}{fn}{value}{ext}"
        };

        return true;
    }
}