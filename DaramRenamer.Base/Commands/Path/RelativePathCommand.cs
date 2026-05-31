using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class RelativePathCommand : ICommand
{
    private const string CurrentDirectory = ".";
    private const string PreviousDirectory = "..";

    public string Path
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
        }
    } = "";

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 2;
    public CommandCategory Category => CommandCategory.Path;
    
    public void Apply(FileItem item, int _)
    {
        var temp = new List<string>(System.IO.Path.Combine(item.ChangedPath, Path).Split('/', '\\'));
        temp.RemoveAll(dir => dir == CurrentDirectory || string.IsNullOrEmpty(dir));

        var indexOf = -1;
        while ((indexOf = temp.IndexOf(PreviousDirectory)) >= 0)
        {
            if (indexOf is 0 or 1)
                return;
            temp.RemoveAt(indexOf);
            temp.RemoveAt(indexOf - 1);
        }

        item.ChangedPath = string.Join(System.IO.Path.DirectorySeparatorChar, temp);
    }
}