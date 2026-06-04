using System.ComponentModel;

namespace DaramRenamer;

public interface ICommand : INotifyPropertyChanged, IOrderBy
{
    CommandCategory Category { get; }
    bool ParallelProcessable => true;
    
    void Apply(FileItem item, int index);

    bool DoCommand(FileItem item, int index = 0)
    {
        Apply(item, index);
        return true;
    }
}
