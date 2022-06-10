using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DaramRenamer.Commands.Filename;

[Serializable]
[LocalizationKey("Command_Name_RearrangeRegexp")]
public class RearrangeRegexpCommand : ICommand, IOrderBy
{
    [LocalizationKey("Command_Argument_RearrangeRegexp_Find")]
    public Regex Regexp { get; set; } = new("$^");

    [LocalizationKey("Command_Argument_RearrangeRegexp_Replace")]
    public string FormatString { get; set; } = string.Empty;

    [LocalizationKey("Command_Argument_RearrangeRegexp_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Filename;

    public bool DoCommand(FileInfo file)
    {
        var match =
            Regexp.Match(IncludeExtension
                ? file.ChangedFilename
                : Path.GetFileNameWithoutExtension(file.ChangedFilename));
        try
        {
            var ext = !IncludeExtension ? Path.GetExtension(file.ChangedFilename) : "";
            var group = match.Groups;
            var groupArr = new object[group.Count];
            for (var i = 0; i < groupArr.Length; i++)
                groupArr[i] = group[i].Value.Trim();
            file.ChangedFilename = $"{string.Format(FormatString, groupArr)}{ext}";
        }
        catch
        {
            return false;
        }

        return true;
    }

    public int Order => int.MinValue + 2;
}