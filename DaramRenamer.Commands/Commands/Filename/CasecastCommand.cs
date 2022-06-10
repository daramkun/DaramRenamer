using System;
using System.IO;
using System.Linq;

namespace DaramRenamer.Commands.Filename;

[Serializable]
[LocalizationKey("Command_Name_Casecast")]
public class CasecastCommand : ICommand, IOrderBy
{
    [LocalizationKey("Command_Argument_Casecast_Casecast")]
    public Casecast2 Casecast { get; set; } = Casecast2.LowercaseAll;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Filename;

    public bool DoCommand(FileInfo file)
    {
        var filename = Path.GetFileNameWithoutExtension(file.ChangedFilename);
        var ext = Path.GetExtension(file.ChangedFilename);
        switch (Casecast)
        {
            case Casecast2.UppercaseAll:
                file.ChangedFilename = $"{filename.ToUpper()}{ext}";
                break;
            case Casecast2.LowercaseAll:
                file.ChangedFilename = $"{filename.ToLower()}{ext}";
                break;
            case Casecast2.UppercaseFirstLetter:
            {
                var fn = filename.Split(' ');
                for (var i = 0; i < fn.Length; ++i)
                {
                    var chars = fn[i].ToArray();
                    chars[0] = char.ToUpper(chars[0]);
                    fn[i] = new string(chars);
                }

                file.ChangedFilename = $"{string.Join(" ", fn)}{ext}";
            }
                break;

            default:
                return false;
        }

        return true;
    }

    public int Order => int.MinValue + 9;
}