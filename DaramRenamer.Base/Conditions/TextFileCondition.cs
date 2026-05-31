using System.ComponentModel;
using DaramRenamer.Helpers;

namespace DaramRenamer.Conditions;

public class TextFileCondition : ICondition
{
    public int Order => int.MinValue + 1;
    
    public bool IsSatisfied(FileItem item)
    {
        if (item.IsDirectory)
            return false;

        return TextFileHelper.IsTextFile(item.SourceFullPath);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}