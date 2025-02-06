using DaramRenamer.Attributes;

namespace DaramRenamer.Conditions;

[Serializable]
[LocalizationKey("Condition_IsFile")]
public class FileCondition : BaseCondition
{
    public override bool IsSatisfied(BaseFileInfo fileInfo) =>
        !fileInfo.IsDirectory;
}