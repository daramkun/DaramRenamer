using System.ComponentModel;
using System.Text.RegularExpressions;

namespace DaramRenamer.Commands;

[Serializable]
public class ArrangeRegexCommand : ICommand
{
    private Regex? _cache;
    
    public string Regex
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            _cache = null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Regex)));
        }
    } = "$^";

    public string FormatString
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FormatString)));
        }
    } = string.Empty;

    public bool IncludeExtension
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncludeExtension)));
        }
    } = false;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 3;
    public CommandCategory Category => CommandCategory.Filename;

    public void Apply(FileItem item, int _)
    {
        _cache ??= new Regex(Regex);
        
        var match =
            _cache.Match(IncludeExtension
                ? item.ChangedName
                : item.ChangedNameWithoutExtension
            );

        try
        {
            var ext = !IncludeExtension ? item.ChangedExtension : string.Empty;
            var group = match.Groups;
            var groupArr = new object[group.Count];
            for (var i = 0; i < groupArr.Length; i++)
                groupArr[i] = group[i].Value.Trim();
            item.ChangedName = $"{string.Format(FormatString, groupArr)}{ext}";
        }
        catch
        {
            // Ignore
        }
    }
}