using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_RelativeGoTo")]
public class RelativeGoToCommand : BaseCommand
{
    private const string CurrentDirectory = ".";
    private const string PreviousDirectory = "..";
    
    public override CommandCategory Category => CommandCategory.Path;
    
    [LocalizationKey("Command_Argument_RelativeGoTo_Path")]
    public string Path { get; set; } = "";
    
    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var temp = new List<string>(System.IO.Path.Combine(fileInfo.ChangedPath, Path).Split('/', '\\'));
        temp.RemoveAll(dir => dir == CurrentDirectory || string.IsNullOrEmpty(dir));

        var indexOf = -1;
        while ((indexOf = temp.IndexOf(PreviousDirectory)) >= 0)
        {
            if (indexOf is 0 or 1)
                return false;
            temp.RemoveAt(indexOf);
            temp.RemoveAt(indexOf - 1);
        }

        fileInfo.ChangedPath = string.Join(System.IO.Path.DirectorySeparatorChar, temp);
        return true;
    }
}