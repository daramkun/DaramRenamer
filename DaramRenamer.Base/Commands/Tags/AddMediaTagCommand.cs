using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace DaramRenamer.Commands;

[Serializable]
public class AddMediaTagCommand : ICommand
{
    public MediaTag Tag
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tag)));
        }
    }

    public int Arguments
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Arguments)));
        }
    } = -1;

    public Position3 Position
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
        }
    } = Position3.End;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 4;
    public CommandCategory Category => CommandCategory.Tag;
    
    public void Apply(FileItem item, int _)
    {
        if (item.IsDirectory)
            return;

        var tag = ConvertUnicodeText(MediaTagReader.Read(item.SourceFullPath, Tag, Arguments));

        var fn = item.ChangedNameWithoutExtension;
        var ext = item.ChangedExtension;
        item.ChangedName = Position switch
        {
            Position3.Begin => $"{tag}{fn}{ext}",
            Position3.End => $"{fn}{tag}{ext}",
            Position3.Both => $"{tag}{fn}{tag}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // Code idea from https://github.com/mildblue/DaramRenamer/commit/d3a2b71c081bacfc30a3b195280f0d10eff08944
    public static string ConvertUnicodeText(string text)
    {
        var latinStr =
            Encoding.Default.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(text.Replace("?", "")));
        return !latinStr.Contains('?') && latinStr.Any(ch =>
            char.GetUnicodeCategory(ch) == UnicodeCategory.OtherLetter)
            ? Encoding.Default.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(text))
            : text;
    }
}
