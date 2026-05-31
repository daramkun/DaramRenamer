using System.ComponentModel;

namespace DaramRenamer.Commands;

[Serializable]
public class DeleteBlockCommand : ICommand
{
    public string StartBlock
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartBlock)));
        }
    } = string.Empty;

    public string EndBlock
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EndBlock)));
        }
    } = string.Empty;

    public bool DeleteAllBlocks
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeleteAllBlocks)));
        }
    } = false;

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

    public int Order => int.MinValue + 7;
    public CommandCategory Category => CommandCategory.Filename;
    
    public void Apply(FileItem item, int _)
    {
        if (string.IsNullOrEmpty(StartBlock) || string.IsNullOrEmpty(EndBlock))
            return;
        
        var filename =
            !IncludeExtension
                ? item.ChangedNameWithoutExtension
                : item.ChangedName;
        var ext =
            !IncludeExtension
                ? item.ChangedExtension
                : string.Empty;

        int first;
        while ((first = filename.IndexOf(StartBlock, StringComparison.Ordinal)) != -1)
        {
            var last = filename.IndexOf(EndBlock, first + 1, StringComparison.Ordinal);
            if (last == -1)
                break;

            filename = filename.Remove(first, last - first + EndBlock.Length);
            if (!DeleteAllBlocks)
                break;
        }

        item.ChangedName = $"{filename}{ext}";
    }
}