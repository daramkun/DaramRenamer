using System;
using System.IO;
using System.Windows.Input;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_ConcatenateDirectoryName")]
public class ConcatenateDirectoryNameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 4;
    
    [LocalizationKey("Command_Argument_ConcatenateDirectoryName_Position")]
    public Position Position { get; set; } = Position.Begin;

    [LocalizationKey("Command_Argument_ConcatenateDirectoryName_ApplyToDirectory")]
    public bool ApplyToDirectory { get; set; } = false;

    [LocalizationKey("Command_Argument_ConcatenateDirectoryName_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (!ApplyToDirectory && fileInfo.IsDirectory)
            return true;

        var filename =
            !IncludeExtension
                ? fileInfo.ChangedNameWithoutExtension
                : fileInfo.ChangedName;
        var ext =
            !IncludeExtension
                ? fileInfo.ChangedNameExtension
                : string.Empty;

        var startIndex = fileInfo.ChangedPath.LastIndexOf('\\');
        if (startIndex < 0)
            startIndex = fileInfo.ChangedPath.LastIndexOf('/');

        var text =
            startIndex >= 0
                ? fileInfo.ChangedPath[(startIndex + 1)..]
                : string.Empty;

        fileInfo.ChangedName =
            Position == Position.Begin
                ? $"{text}{filename}{ext}"
                : Position == Position.End
                    ? $"{filename}{text}{ext}"
                    : $"{text}{filename}{text}{ext}";

        return true;
    }
}