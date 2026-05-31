using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class SubstringCommand : ICommand
{
    public uint StartIndex
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartIndex)));
        }
    } = 0;

    public uint? Length
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length)));
        }
    } = null;

    public bool IncludeExtension
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncludeExtension)));
        }
    } = false;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 9;
    public CommandCategory Category => CommandCategory.Filename;
    
    public void Apply(FileItem item, int _)
    {
        var baseFilename = IncludeExtension
            ? item.ChangedName
            : item.ChangedNameWithoutExtension;
        
        var startIndex = (int) StartIndex;
        if (startIndex >= baseFilename.Length)
            return;
        
        var length = (int?) Length;
        if (length != null && startIndex + length >= baseFilename.Length)
            length = null;
        
        item.ChangedName =
            length == null
                ? IncludeExtension
                    ? baseFilename[startIndex..]
                    : $"{baseFilename[startIndex..]}{item.ChangedExtension}"
                : IncludeExtension
                    ? baseFilename.Substring(startIndex, length.Value)
                    : $"{baseFilename.Substring(startIndex, length.Value)}{item.ChangedExtension}";
    }
}