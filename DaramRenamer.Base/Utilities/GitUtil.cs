namespace DaramRenamer.Utilities;

public enum GitDetermineKind
{
    CommitId,
    ShortCommitId,
    BranchName,
    CommitCount,
}

public static class GitUtil
{
    private static readonly string[] Arguments =
    [
        "rev-parse HEAD",
        "rev-parse --short HEAD",
        "branch",
        "rev-list --count --all"
    ];

    public static string? Determine(GitDetermineKind kind, string workingDirectory)
    {
        var result = ProcessUtil.StartProcessAndReturnStdOut("git", Arguments[(int)kind], workingDirectory);
        if (result == null)
            return null;
        
        if (kind != GitDetermineKind.BranchName)
            return result;
        
        var list = result.Split('\n');
        foreach (var item in list)
        {
            if (item[0] != '*')
                continue;
            result = item[2..];
            break;
        }

        return result;
    }
}