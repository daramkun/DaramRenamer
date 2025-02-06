using DaramRenamer.Attributes;
using DaramRenamer.Utilities;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_CasecastExtension")]
public class CasecastExtensionCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Extension;
    public override int Order => int.MinValue + 4;
    
    [LocalizationKey("Command_Argument_CasecastExtension_Casecast")]
    public CasecastKind Casecast { get; set; } = CasecastKind.Lowercase;

    [LocalizationKey("Commamd_Argument_CastcastExtension_ApplyToDirectory")]
    public bool ApplyToDirectory { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (!ApplyToDirectory && fileInfo.IsDirectory)
            return true;

        var filename = fileInfo.ChangedNameWithoutExtension;
        var ext = fileInfo.ChangedNameExtension;
        
        fileInfo.ChangedName = Casecast switch
        {
            CasecastKind.Uppercase => $"{filename.ToUpper()}{ext}",
            CasecastKind.Lowercase => $"{filename.ToLower()}{ext}",
            CasecastKind.UppercaseFirstLetterOnly => $"{filename.ToUpperFirstLetter()}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };

        return true;
    }
}