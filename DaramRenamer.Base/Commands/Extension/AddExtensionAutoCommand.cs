using System.ComponentModel;
using DaramRenamer.FileTypes;

namespace DaramRenamer.Commands;

[Serializable]
public class AddExtensionAutoCommand : ICommand
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Order => int.MinValue + 2;
    public CommandCategory Category => CommandCategory.Extension;

    public void Apply(FileItem item, int index)
    {
        if (item.IsDirectory)
            return;

        if (!File.Exists(item.SourceFullPath))
            return;

        using Stream stream = File.OpenRead(item.SourceFullPath);
        var detector = FileTypeDetector.Detect(stream);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (detector == null)
            return;

        item.ChangedName = $"{item.ChangedName}.{detector.Extension}";
    }
}
