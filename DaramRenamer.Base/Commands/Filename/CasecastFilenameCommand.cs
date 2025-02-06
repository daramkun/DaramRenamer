using DaramRenamer.Attributes;
using DaramRenamer.Utilities;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_Casecast")]
public class CasecastFilenameCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Filename;
    public override int Order => int.MinValue + 9;
    
    [LocalizationKey("Command_Argument_Casecast_Casecast")]
    public CasecastKind Casecast { get; set; } = CasecastKind.Lowercase;
    
    public override bool DoCommand(BaseFileInfo fileInfo)
    {
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

    private static string ToUpperFirstLetter(string filename)
    {
        Span<char> buffer = stackalloc char[filename.Length];
        filename.CopyTo(buffer);
        
        var isFirstLetter = true;
        for (var i = 0; i < buffer.Length; ++i)
        {
            var ch = buffer[i];
            if (char.IsUpper(ch) || char.IsLower(ch))
            {
                if (isFirstLetter)
                    buffer[i] = char.ToUpper(ch);
                isFirstLetter = false;
            }
            else
                isFirstLetter = true;
        }

        return new string(buffer);
    }
}