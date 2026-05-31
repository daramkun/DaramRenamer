
using System.ComponentModel;
using Cysharp.Text;

namespace DaramRenamer.Commands;

[Serializable]
public class DeleteNoNumberCommand : ICommand
{
    private static readonly char[] Spliters =
    [
        ' ', '[', ']', ',', '.', '(', ')', '{', '}', '<', '>', '　',
        '\t', ':', ';', '*', '&', '@', '^', '-', '_', '=', '+', '~'
    ];

    public bool Wordly
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wordly)));
        }
    } = false;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 2;
    public CommandCategory Category => CommandCategory.Number;
    
    public void Apply(FileItem item, int index)
    {
        if (item.ChangedName.Length == 0)
            return;

        var sb = ZString.CreateStringBuilder();
        if (!Wordly)
        {
            foreach (var ch in item.ChangedNameWithoutExtension
                         .Where(ch => ch is >= '0' and <= '9'))
                sb.Append(ch);
        }
        else
        {
            foreach (var str in item.ChangedNameWithoutExtension.Split(Spliters))
            {
                foreach (var ch in str.Where(ch => ch is >= '0' and <= '9'))
                    sb.Append(ch);
                sb.Append(' ');
            }

            sb.Remove(sb.Length - 1, 1);
        }

        item.ChangedName = $"{sb}{item.ChangedExtension}";
    }
}