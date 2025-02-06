using System.Collections.ObjectModel;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AddIndex")]
public class AddIndexCommand : BaseCommand, ITargetContains
{
    private IEnumerable<BaseFileInfo> _targets = [];

    [LocalizationKey("Command_Argument_AddIndex_Position")]
    public Direction Direction { get; set; } = Direction.BottomUp;

    public override CommandCategory Category => CommandCategory.Number;

    public override bool DoCommand(BaseFileInfo fileInfo)
    {
        var index = _targets is ObservableCollection<BaseFileInfo> observableCollection
            ? observableCollection.IndexOf(fileInfo) + 1
            : _targets.TakeWhile(x => !Equals(x, fileInfo)).Count();
        fileInfo.ChangedName = string.Format(Direction switch
            {
                Direction.BottomUp => "{0}{1}{2}",
                Direction.TopDown => "{1}{0}{2}",
                _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null),
            },
            fileInfo.ChangedNameWithoutExtension, index, fileInfo.ChangedNameExtension);
        return true;
    }

    public void SetTargets(IEnumerable<BaseFileInfo> files)
    {
        _targets = files;
    }
}