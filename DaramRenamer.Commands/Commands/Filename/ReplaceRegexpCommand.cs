using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DaramRenamer.Commands.Filename;

[Serializable]
[LocalizationKey("Command_Name_ReplaceRegexp")]
public class ReplaceRegexpCommand : ICommand, IOrderBy
{
    [LocalizationKey("Command_Argument_ReplaceRegexp_Find")]
    public Regex Find { get; set; } = new("$^");

    [LocalizationKey("Command_Argument_ReplaceRegexp_Replace")]
    public string Replace { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_ReplaceRegexp_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Filename;

    public bool DoCommand(FileInfo file)
    {
        var filename =
            !IncludeExtension
                ? Path.GetFileNameWithoutExtension(file.ChangedFilename)
                : file.ChangedFilename;
        var ext =
            !IncludeExtension
                ? Path.GetExtension(file.ChangedFilename)
                : "";
        file.ChangedFilename = $"{Find.Replace(filename, Replace)}{ext}";
        return true;
    }

    public int Order => int.MinValue + 1;
}