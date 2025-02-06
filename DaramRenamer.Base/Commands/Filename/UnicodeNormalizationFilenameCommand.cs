using System.Text;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_NF")]
public class UnicodeNormalizationFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 10;

    [LocalizationKey("Command_Argument_NF_Normalization")]
    public UnicodeNormalization Normalization { get; set; } = UnicodeNormalization.NFC;
    [LocalizationKey("Command_Argument_NF_IncludeExtension")]
    public bool IncludeExtension { get; set; } = false;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var filename = IncludeExtension ? fileInfo.ChangedName : fileInfo.ChangedNameWithoutExtension;
        var ext = IncludeExtension ? fileInfo.ChangedNameExtension : string.Empty;

        fileInfo.ChangedName = filename.Normalize(Normalization switch
        {
            UnicodeNormalization.NFD => NormalizationForm.FormKD,
            UnicodeNormalization.NFC => NormalizationForm.FormKC,
            _ => throw new ArgumentOutOfRangeException(nameof(Normalization), Normalization, null)
        }) + ext;

        return true;
    }
}