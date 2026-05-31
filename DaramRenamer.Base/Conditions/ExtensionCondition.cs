using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DaramRenamer.Conditions;

[Serializable]
public class ExtensionCondition : ICondition
{
    public int Order => int.MinValue + 4;
    
    public string Extension
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public bool IsSatisfied(FileItem item)
    {
        return !item.IsDirectory && Extension.Split(',').Contains(Path.GetExtension(item.ChangedName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}