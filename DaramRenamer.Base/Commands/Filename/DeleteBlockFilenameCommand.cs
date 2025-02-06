using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_DeleteBlock")]
public class DeleteBlockFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 6;
    
    [LocalizationKey("Command_Argument_DeleteBlock_StartBlock")]
    public string StartBlock { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_DeleteBlock_EndBlock")]
    public string EndBlock { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_DeleteBlock_DeleteAllBlocks")]
    public bool DeleteAllBlocks { get; set; } = false;

    [LocalizationKey("Command_Argument_DeleteBlock_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;
    
    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (string.IsNullOrEmpty(StartBlock) || string.IsNullOrEmpty(EndBlock))
            return false;

        var filename = IncludeExtension ? fileInfo.ChangedName : fileInfo.ChangedNameWithoutExtension;
        var ext = IncludeExtension ? string.Empty : fileInfo.ChangedNameExtension;

        int first;
        while ((first = filename.IndexOf(StartBlock, StringComparison.Ordinal)) >= 0)
        {
            var last = filename.IndexOf(EndBlock, first + 1, StringComparison.Ordinal);
            if (last == -1)
                break;

            filename = filename.Remove(first, last - first + EndBlock.Length);
            if (!DeleteAllBlocks)
                break;
        }
        
        fileInfo.ChangedName = $"{filename}{ext}";

        return true;
    }
}