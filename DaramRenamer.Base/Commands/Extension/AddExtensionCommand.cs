using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class AddExtensionCommand : ICommand
{
    public string Extension
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Extension)));
        }
    } = string.Empty;

    public bool ApplyToDirectory
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApplyToDirectory)));
        }
    } = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 1;
    public CommandCategory Category => CommandCategory.Extension;
    
    public void Apply(FileItem item, int index)
    {
        if (string.IsNullOrEmpty(Extension))
            return;

        if (!ApplyToDirectory && item.IsDirectory)
            return;

        item.ChangedName = $"{item.ChangedName}{(Extension[0] != '.' ? "." : "")}{Extension}";
    }
}