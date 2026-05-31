using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DaramRenamer.Conditions;

[Serializable]
public class DirectoryCondition : ICondition
{
    public int Order => int.MinValue + 3;
    
    public bool IsSatisfied(FileItem item) =>
        item.IsDirectory;

    public event PropertyChangedEventHandler? PropertyChanged;
}