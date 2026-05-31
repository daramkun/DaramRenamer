using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class ManualEditCommand : ICommand
{
    public string ChangeName
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChangeName)));
        }
    } = string.Empty;

    public string ChangePath
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChangePath)));
        }
    } = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue;
    public CommandCategory Category => CommandCategory.NoCategorized;

    public void Apply(FileItem item, int index)
    {
        item.ChangedName = ChangeName;
        item.ChangedPath = ChangePath;
    }
}
