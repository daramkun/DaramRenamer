using System.ComponentModel;
using System.Text.RegularExpressions;

namespace DaramRenamer.Commands;

[Serializable]
public class ReplaceRegexCommand : ICommand
{
    private Regex? _cache;
    
    public string Find
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            _cache = null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Find)));
        }
    } = "$^";

    public string Replace { get; set; } = string.Empty;

    public bool IncludeExtension { get; set; } = false;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 2;
    public CommandCategory Category => CommandCategory.Filename;

    public void Apply(FileItem item, int _)
    {
        _cache ??= new Regex(Find);
        
        var filename =
            !IncludeExtension
                ? item.ChangedNameWithoutExtension
                : item.ChangedName;
        var ext =
            !IncludeExtension
                ? item.ChangedExtension
                : string.Empty;
        
        item.ChangedName = $"{_cache.Replace(filename, Replace)}{ext}";
    }
}