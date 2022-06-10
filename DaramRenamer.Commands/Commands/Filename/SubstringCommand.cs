using System;
using System.IO;

namespace DaramRenamer.Commands.Filename;

[Serializable]
[LocalizationKey("Command_Name_Substring")]
public class SubstringCommand : ICommand, IOrderBy
{
    [LocalizationKey("Command_Argument_Substring_StartIndex")]
    public uint StartIndex { get; set; } = 0;

    [LocalizationKey("Command_Argument_Substring_Length")]
    public uint? Length { get; set; } = null;

    [LocalizationKey("Command_Argument_Substring_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Filename;

    public bool DoCommand(FileInfo file)
    {
        var baseFilename = IncludeExtension
            ? file.ChangedFilename
            : Path.GetFileNameWithoutExtension(file.ChangedFilename);
        
        var startIndex = (int) StartIndex;
        if (startIndex >= baseFilename.Length)
            return false;
        
        var length = (int?) Length;
        if (length != null && startIndex + length >= baseFilename.Length)
            length = null;
        
        file.ChangedFilename =
            length == null
                ? IncludeExtension
                    ? baseFilename[startIndex..]
                    : $"{baseFilename[startIndex..]}{Path.GetExtension(file.ChangedFilename)}"
                : IncludeExtension
                    ? baseFilename.Substring(startIndex, length.Value)
                    : $"{baseFilename.Substring(startIndex, length.Value)}{Path.GetExtension(file.ChangedFilename)}";
        return true;
    }

    public int Order => int.MinValue + 8;
}