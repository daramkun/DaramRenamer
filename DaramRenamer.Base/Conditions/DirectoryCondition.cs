using DaramRenamer.Attributes;

namespace DaramRenamer.Conditions;

[Serializable]
[LocalizationKey("Condition_IsDirectory")]
public class DirectoryCondition : BaseCondition
{
    public override bool IsSatisfied(BaseFileInfo fileInfo) =>
        fileInfo.IsDirectory;
}