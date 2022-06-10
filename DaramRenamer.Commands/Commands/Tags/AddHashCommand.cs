using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DaramRenamer.Commands.Tags;

[Serializable]
[LocalizationKey("Command_Name_AddHash")]
public class AddHashCommand : ICommand
{
    [LocalizationKey("Command_Argument_AddHash_HashType")]
    public HashType HashType { get; set; }

    [LocalizationKey("Command_Argument_AddHash_Position")]
    public Position1 Position { get; set; } = Position1.EndPoint;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Tag;

    public bool DoCommand(FileInfo file)
    {
        var hash = ComputeHash(HashType, file.OriginalFullPath);

        var fn = Path.GetFileNameWithoutExtension(file.ChangedFilename);
        var ext = Path.GetExtension(file.ChangedFilename);
        file.ChangedFilename = Position switch
        {
            Position1.StartPoint => $"{hash}{fn}{ext}",
            Position1.EndPoint => $"{fn}{hash}{ext}",
            _ => $"{hash}{fn}{hash}{ext}"
        };

        return true;
    }

    public static string ComputeHash(HashType hashType, string path)
    {
        byte[] returnValue = null;
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