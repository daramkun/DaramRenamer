using System;
using System.IO;

namespace DaramRenamer.Commands.Filename;

[Serializable]
[LocalizationKey("Command_Name_DeleteFilename")]
public class DeleteFilenameCommand : ICommand, IOrderBy
{
    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Filename;

    public bool DoCommand(FileInfo file)
    {
        file.ChangedFilename = Path.GetExtension(file.ChangedFilename);
        return true;
    }

    public int Order => int.MinValue + 7;
}