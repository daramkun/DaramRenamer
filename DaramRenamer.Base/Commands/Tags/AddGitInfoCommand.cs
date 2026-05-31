using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace DaramRenamer.Commands;

[Serializable]
public class AddGitInfoCommand : ICommand
{
    public GitInfo GitInfo
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GitInfo)));
        }
    } = GitInfo.CommitId;

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
    
    public int Order => int.MinValue + 2;
    public CommandCategory Category => CommandCategory.Tag;
    
    public void Apply(FileItem item, int index)
    {
        var value = GetGitValue(GitInfo, item.SourceFullPath);
        if (string.IsNullOrEmpty(value))
            return;

        var fn = item.ChangedNameWithoutExtension;
        var ext = item.ChangedExtension;
        item.ChangedName = Position switch
        {
            Position3.Begin => $"{value}{fn}{ext}",
            Position3.End => $"{fn}{value}{ext}",
            Position3.Both => $"{value}{fn}{value}{ext}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private static readonly string[] Arguments =
    [
        "rev-parse HEAD",
        "rev-parse --short HEAD",
        "branch",
        "rev-list --count --all"
    ];
    
    public static string GetGitValue(GitInfo gitInfo, string path)
    {
        var psInfo = new ProcessStartInfo("git", Arguments[(int) gitInfo])
        {
            UseShellExecute = false,
            WorkingDirectory = path,
            RedirectStandardOutput = true,
            StandardOutputEncoding = Encoding.UTF8,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        };

        Process? process;
        try
        {
            process = Process.Start(psInfo);
            process?.WaitForExit();
            if (process?.ExitCode != 0)
                return string.Empty;
        }
        catch
        {
            return string.Empty;
        }

        var value = process.StandardOutput.ReadToEnd().Trim();
        if (gitInfo != GitInfo.BranchName)
            return value;

        var list = value.Split('\n');
        foreach (var item in list)
        {
            if (item[0] != '*')
                continue;
            value = item[2..];
            break;
        }

        return value;
    }
}