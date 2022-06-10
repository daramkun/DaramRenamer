namespace DaramRenamer.Test;

public static class TestUtil
{
    public static FileInfo MakeFileInfo(string fullPath)
    {
        return new FileInfo(fullPath, directoryCheck: false);
    }
    
    public static void DoCommand(this ICommand command, params FileInfo[] fileInfos)
    {
        foreach (var fileInfo in fileInfos)
            command.DoCommand(fileInfo);
    }
}