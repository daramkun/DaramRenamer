using System.Security.Cryptography;
using System.Text;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AddHash")]
public class AddHashCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Tag;
    
    [LocalizationKey("Command_Argument_AddHash_HashType")]
    public HashType HashType { get; set; }

    [LocalizationKey("Command_Argument_AddHash_Position")]
    public Position Position { get; set; } = Position.End;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        if (fileInfo.IsDirectory)
            return true;
        
        var hash = ComputeHash(HashType, fileInfo.OriginalFullPath);

        var fn = fileInfo.ChangedNameWithoutExtension;
        var ext = fileInfo.ChangedNameExtension;
        fileInfo.ChangedName = Position switch
        {
            Position.Begin => $"{hash}{fn}{ext}",
            Position.End => $"{fn}{hash}{ext}",
            Position.Both => $"{hash}{fn}{hash}{ext}",
            _ => $"{hash}{fn}{hash}{ext}"
        };

        return true;
    }

    public static string ComputeHash(HashType hashType, string path)
    {
        byte[]? returnValue = null;
        switch (hashType)
        {
            case HashType.MD5:
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    returnValue = MD5.Create().ComputeHash(stream);
                }

                break;
            case HashType.SHA1:
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    returnValue = SHA1.Create().ComputeHash(stream);
                }

                break;
            case HashType.SHA256:
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    returnValue = SHA256.Create().ComputeHash(stream);
                }

                break;
            case HashType.SHA384:
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    returnValue = SHA384.Create().ComputeHash(stream);
                }

                break;
            case HashType.SHA512:
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    returnValue = SHA512.Create().ComputeHash(stream);
                }

                break;
        }

        var sBuilder = new StringBuilder();
        if (returnValue != null)
            foreach (var t in returnValue)
                sBuilder.Append(t.ToString("x2"));

        return sBuilder.ToString();
    }
}