using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DaramRenamer.Commands.Number;

[Serializable]
[LocalizationKey("Command_Name_AddIndex")]
public class AddIndexCommand : ICommand, ITargetContains
{
    private IEnumerable<FileInfo> targets;

    [LocalizationKey("Command_Argument_AddIndex_Position")]
    public Position1 Position { get; set; } = Position1.EndPoint;

    public bool ParallelProcessable => false;
    public CommandCategory Category => CommandCategory.Number;

    public bool DoCommand(FileInfo file)
    {
        var index = targets is ObservableCollection<FileInfo> observableCollection
            ? observableCollection.IndexOf(file) + 1
            : targets.TakeWhile(x => !Equals(x, file)).Count();
        file.ChangedFilename = string.Format(Position == Position1.EndPoint ? "{0}{1}{2}" : "{1}{0}{2}",
            Path.GetFileNameWithoutExtension(file.ChangedFilename), index, Path.GetExtension(file.ChangedFilename));
        return true;
    }

    public void SetTargets(IEnumerable<FileInfo> files)
    {
        targets = files;
    }
}