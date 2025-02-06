using DaramRenamer.Attributes;

namespace DaramRenamer.Conditions;

[Serializable]
[LocalizationKey("Condition_Name_Extension")]
public class ExtensionCondition : BaseCondition
{
    [LocalizationKey("Condition_Argument_Extension_Extension")]
    public string Extension { get; set; } = "";

    public override bool IsSatisfied(BaseFileInfo file)
    {
        return !file.IsDirectory &&
               Extension.Split(',').Contains(Path.GetExtension(file.ChangedName));
    }
}