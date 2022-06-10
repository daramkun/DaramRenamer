using System;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_ManualEdit")]
public class ManualEditCommand : ICommand
{
    [LocalizationKey("Command_Argument_ManualEdit_ChangeName")]
    public string ChangeName { get; set; }

    [LocalizationKey("Command_Argument_ManualEdit_ChangePath")]
    public string ChangePath { get; set; }

    public bool ParallelProcessable => false;
    public CommandCategory Category => CommandCategory.NoCategorized;

    public bool DoCommand(FileInfo file)
    {
        file.ChangedFilename = ChangeName;
        file.ChangedPath = ChangePath;
        return true;
    }
}