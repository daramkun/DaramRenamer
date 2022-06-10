using System;
using System.IO;

namespace DaramRenamer.Commands.Filename;

[Serializable]
[LocalizationKey("Command_Name_DeleteBlock")]
public class DeleteBlockCommand : ICommand, IOrderBy
{
    [LocalizationKey("Command_Argument_DeleteBlock_StartBlock")]
    public string StartBlock { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_DeleteBlock_EndBlock")]
    public string EndBlock { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_DeleteBlock_DeleteAllBlocks")]
    public bool DeleteAllBlocks { get; set; } = false;

    [LocalizationKey("Command_Argument_DeleteBlock_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Filename;

    public bool DoCommand(FileInfo file)
    {
        if (string.IsNullOrEmpty(StartBlock)) return false;
        if (string.IsNullOrEmpty(EndBlock)) return false;
        var filename =
            !IncludeExtension
                ? Path.GetFileNameWithoutExtension(file.ChangedFilename)
                : file.ChangedFilename;
        var ext =
            !IncludeExtension
                ? Path.GetExtension(file.ChangedFilename)
                : string.Empty;

        int first;
        while ((first = filename.IndexOf(StartBlock, StringComparison.Ordinal)) != -1)
        {
            var last = filename.IndexOf(EndBlock, first + 1, StringComparison.Ordinal);
            if (last == -1)
                break;

            filename = filename.Remove(first, last - first + EndBlock.Length);
            if (!DeleteAllBlocks)
                break;
        }

        file.ChangedFilename = $"{filename}{ext}";

        return true;
    }

    public int Order => int.MinValue + 6;
}