using System;
using System.IO;
using System.Windows.Input;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_Substring")]
public class SubstringFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 8;
    
    [LocalizationKey("Command_Argument_Substring_StartIndex")]
    public uint StartIndex { get; set; } = 0;

    [LocalizationKey("Command_Argument_Substring_Length")]
    public uint? Length { get; set; } = null;

    [LocalizationKey("Command_Argument_Substring_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var baseFilename = IncludeExtension
            ? fileInfo.ChangedName
            : fileInfo.ChangedNameWithoutExtension;
        
        var startIndex = (int) StartIndex;
        if (startIndex >= baseFilename.Length)
            return false;
        
        var length = (int?) Length;
        if (length != null && startIndex + length >= baseFilename.Length)
            length = null;
        
        fileInfo.ChangedName =
            length == null
                ? IncludeExtension
                    ? baseFilename[startIndex..]
                    : $"{baseFilename[startIndex..]}{fileInfo.ChangedNameExtension}"
                : IncludeExtension
                    ? baseFilename.Substring(startIndex, length.Value)
                    : $"{baseFilename.Substring(startIndex, length.Value)}{fileInfo.ChangedNameExtension}";
        return true;
    }
}