using System.ComponentModel;

namespace DaramRenamer.Conditions;

[Serializable]
public class FileCondition : ICondition
{
    public int Order => int.MinValue + 2;
    
    public bool IsSatisfied(FileItem item) =>
        !item.IsDirectory;
    
    public event PropertyChangedEventHandler? PropertyChanged;
}