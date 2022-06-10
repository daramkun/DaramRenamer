using System;
using System.IO;

namespace DaramRenamer.Commands.FilePath;

[Serializable]
[LocalizationKey("Command_Name_AbsoluteGoTo")]
public class AbsoluteGoToCommand : ICommand
{
    [LocalizationKey("Command_Argument_AbsoluteGoTo_Path")]
    public DirectoryInfo Path { get; set; } = new(Environment.CurrentDirectory);

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Path;

    public bool DoCommand(FileInfo file)
    {
        if (!Path.Exists) return false;
        file.ChangedPath = Path.FullName;
        return true;
    }
}