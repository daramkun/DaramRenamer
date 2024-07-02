using System;

namespace DaramRenamer.Conditions;

[Serializable]
[LocalizationKey("Condition_IsFile")]
public class FileCondition : ICondition
{
    public bool IsSatisfyThisCondition(FileInfo file)
    {
        return !file.IsDirectory;
    }
}