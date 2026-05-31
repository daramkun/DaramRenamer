using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class ReplacePlainCommand : ICommand
{
    public string Find
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Find)));
        }
    } = string.Empty;

    public string Replace
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Replace)));
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

    public int Order => int.MinValue + 1;
    public CommandCategory Category => CommandCategory.Filename;
    
    public void Apply(FileItem item, int _)
    {
        if (string.IsNullOrEmpty(Find))
            return;

        Replace ??= "";

        item.ChangedName =
            IncludeExtension
                ? $"{item.ChangedName.Replace(Find, Replace)}"
                : $"{item.ChangedNameWithoutExtension.Replace(Find, Replace)}{item.ChangedExtension}";
    }
}