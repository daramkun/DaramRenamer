using System.ComponentModel;

namespace DaramRenamer;

public interface ICondition : INotifyPropertyChanged, IOrderBy
{
    bool IsSatisfied(FileItem item);

    bool IsSatisfyThisCondition(FileItem item) => IsSatisfied(item);
}
