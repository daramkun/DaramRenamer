using System.ComponentModel;
using System.Text.RegularExpressions;

namespace DaramRenamer.Commands;

[Serializable]
public class DeleteDateCommand : ICommand
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 2;
    public CommandCategory Category => CommandCategory.Date;

    public void Apply(FileItem item, int _)
    {
        foreach (var regex in DateRegexps)
        {
            var proceed = regex.Replace(item.ChangedName, string.Empty);
            if (proceed == item.ChangedName)
                continue;
            item.ChangedName = proceed;
            break;
        }
    }

    private static readonly Regex[] DateRegexps =
    {
        new("[0-9][0-9][0-9][0-9][0-1][0-9][0-3][0-9]"),
        new("[0-9][0-9][0-9][0-9][0-3][0-9][0-1][0-9]"),
        new("[0-9][0-9][0-1][0-9][0-3][0-9]"),
        new("[0-9][0-9][0-3][0-9][0-1][0-9]"),
        new("[0-9]?[0-9]?[0-9][0-9]/[0-1]?[0-9]/[0-3]?[0-9]"),
        new("[0-9]?[0-9]?[0-9][0-9]/[0-3]?[0-9]/[0-1]?[0-9]"),
        new("[0-9][0-9]?[0-9][0-9]-[0-1]?[0-9]-[0-3]?[0-9]"),
        new("[0-9][0-9]?[0-9][0-9]-[0-3]?[0-9]-[0-1]?[0-9]"),
        new("((Sun)|(Mon)|(Tue)|(Wed)|(Thu)|(Fri)|(Sat)), [0-9][0-9] ((Jan)|(Fab)|(Mar)|(Apr)|(May)|(Jun)|(Jul)|(Aug)|(Sep)|(Oct)|(Nov)|(Dec)) [0-9][0-9][0-9][0-9] [0-2][0-9]:[0-6][0-9]:[0-6][0-9] [A-Z][A-Z][A-Z]")
    };
}