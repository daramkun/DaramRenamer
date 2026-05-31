using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class ConvensionExtensionCommand : ICommand
{
    public ConvensionBinary Convension
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Convension)));
        }
    } = ConvensionBinary.Lower;

    public bool ApplyToDirectory
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApplyToDirectory)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 5;
    public CommandCategory Category => CommandCategory.Extension;
    
    public void Apply(FileItem item, int index)
    {
        if (!ApplyToDirectory && item.IsDirectory)
            return;

        var name = item.ChangedNameWithoutExtension;
        var ext = item.ChangedExtension;
        item.ChangedName = Convension switch
        {
            ConvensionBinary.Upper => $"{name}{ext.ToUpper()}",
            ConvensionBinary.Lower => $"{name}{ext.ToLower()}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}