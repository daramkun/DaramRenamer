using System;
using System.IO;

namespace DaramRenamer.Commands.Filename;

[Serializable]
[LocalizationKey("Command_Name_ConcatenateDirectoryName")]
internal class ConcatenateDirectoryNameCommand : ICommand, IOrderBy
{
    [LocalizationKey("Command_Argument_ConcatenateDirectoryName_Position")]
    public Position1 Position { get; set; } = Position1.StartPoint;

    [LocalizationKey("Commamd_Argument_ConcatenateDirectoryName_ApplyToDirectory")]
    public bool ApplyToDirectory { get; set; } = false;

    [LocalizationKey("Command_Argument_ConcatenateDirectoryName_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Filename;

    public bool DoCommand(FileInfo file)
    {
        if (!ApplyToDirectory && file.IsDirectory)
            return true;

        var filename =
            !IncludeExtension
                ? Path.GetFileNameWithoutExtension(file.ChangedFilename)
                : file.ChangedFilename;
        var ext =
            !IncludeExtension
                ? Path.GetExtension(file.ChangedFilename)
                : "";

        var startIndex = file.ChangedPath.LastIndexOf('\\');
        if (startIndex < 0)
            startIndex = file.ChangedPath.LastIndexOf('/');

        var text =
            startIndex >= 0
                ? file.ChangedPath.Substring(startIndex + 1)
                : "";

        file.ChangedFilename =
            Position == Position1.StartPoint
                ? $"{text}{filename}{ext}"
                : Position == Position1.EndPoint
                    ? $"{filename}{text}{ext}"
                    : $"{text}{filename}{text}{ext}";

        return true;
    }

    public int Order => int.MinValue + 4;
}