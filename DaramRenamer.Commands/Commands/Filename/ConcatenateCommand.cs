using System;
using System.IO;

namespace DaramRenamer.Commands.Filename;

[Serializable]
[LocalizationKey("Command_Name_Concatenate")]
public class ConcatenateCommand : ICommand, IOrderBy
{
    [LocalizationKey("Command_Argument_Concatenate_Text")]
    public string Text { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_Concatenate_Position")]
    public Position1 Position { get; set; } = Position1.StartPoint;

    [LocalizationKey("Command_Argument_Concatenate_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Filename;

    public bool DoCommand(FileInfo file)
    {
        if (string.IsNullOrEmpty(Text))
            return false;

        var filename =
            !IncludeExtension
                ? Path.GetFileNameWithoutExtension(file.ChangedFilename)
                : file.ChangedFilename;
        var ext =
            !IncludeExtension
                ? Path.GetExtension(file.ChangedFilename)
                : "";

        file.ChangedFilename =
            Position == Position1.StartPoint
                ? $"{Text}{filename}{ext}"
                : Position == Position1.EndPoint
                    ? $"{filename}{Text}{ext}"
                    : $"{Text}{filename}{Text}{ext}";
        return true;
    }

    public int Order => int.MinValue + 3;
}