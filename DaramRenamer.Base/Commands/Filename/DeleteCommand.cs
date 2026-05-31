using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class DeleteCommand : ICommand
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 8;
    public CommandCategory Category => CommandCategory.Filename;
    
    public void Apply(FileItem item, int _)
    {
        item.ChangedName = item.ChangedExtension;
    }
}