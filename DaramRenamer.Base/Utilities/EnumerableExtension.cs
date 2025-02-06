namespace DaramRenamer.Utilities;

public static class EnumerableExtension
{
    public static int IndexOf(this IEnumerable<BaseFileInfo> files, BaseFileInfo fileInfo)
    {
        var i = -1;
        foreach (var file in files)
        {
            ++i;
            if (file.Equals(fileInfo))
                return i;
        }

        return -1;
    }
}