using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class DeleteExtensionCommand : ICommand
{
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

    public int Order => int.MinValue + 3;
    public CommandCategory Category => CommandCategory.Extension;

    public void Apply(FileItem item, int index)
    {
        if (!ApplyToDirectory && item.IsDirectory)
            return;

        item.ChangedName = item.ChangedNameWithoutExtension;
    }
}