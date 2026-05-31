using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class AbsolutePathCommand : ICommand
{
    public string Path
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
        }
    } = Environment.CurrentDirectory;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 1;
    public CommandCategory Category => CommandCategory.Path;
    
    public void Apply(FileItem item, int _)
    {
        if (!System.IO.Path.Exists(item.ChangedPath))
            return;
        item.ChangedPath = System.IO.Path.GetFullPath(Path);
    }
}