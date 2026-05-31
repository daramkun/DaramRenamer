using System.ComponentModel;

namespace DaramRenamer;

public interface ICommand : INotifyPropertyChanged, IOrderBy
{
    CommandCategory Category { get; }
    
    void Apply(FileItem item, int index);
}