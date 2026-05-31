using System.ComponentModel;
using System.IO.Hashing;
using System.Security.Cryptography;
using Cysharp.Text;

namespace DaramRenamer.Commands;

[Serializable]
public class AddHashCommand : ICommand
{
    public HashKind HashKind { get; set; }

    public Position3 Position { get; set; } = Position3.End;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 3;
    public CommandCategory Category => CommandCategory.Tag;
    
    public void Apply(FileItem item, int index)
    {
        if (item.IsDirectory)
            return;
        
        var hash = ComputeHash(HashKind, item.SourceFullPath);

        var fn = item.ChangedNameWithoutExtension;
        var ext = item.ChangedExtension;
        item.ChangedName = Position switch
        {
            Position3.Begin => $"{hash}{fn}{ext}",
            Position3.End => $"{fn}{hash}{ext}",
            Position3.Both => $"{hash}{fn}{hash}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public static string ComputeHash(HashKind kind, string path)
    {
        var returnValue = kind switch
        {
            HashKind.MD5 => MD5.HashData(File.ReadAllBytes(path)),
            HashKind.SHA1 => SHA1.HashData(File.ReadAllBytes(path)),
            HashKind.SHA256 => SHA256.HashData(File.ReadAllBytes(path)),
            HashKind.SHA384 => SHA384.HashData(File.ReadAllBytes(path)),
            HashKind.SHA512 => SHA512.HashData(File.ReadAllBytes(path)),
            HashKind.xxHash64 => XxHash64.Hash(File.ReadAllBytes(path)),
            HashKind.xxHash3 => XxHash3.Hash(File.ReadAllBytes(path)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

        var sBuilder = ZString.CreateStringBuilder();
            foreach (var t in returnValue)
                sBuilder.Append(t.ToString("x2"));

        return sBuilder.ToString();
    }
}