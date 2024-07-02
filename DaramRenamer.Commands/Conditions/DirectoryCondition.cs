using System;

namespace DaramRenamer.Conditions;

[Serializable]
[LocalizationKey("Condition_IsDirectory")]
public class DirectoryCondition : ICondition
{
    public bool IsSatisfyThisCondition(FileInfo file)
    {
        return file.IsDirectory;
    }
}