using System.ComponentModel;
using Daramee.FileTypeDetector;

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
        var detector = DetectorService.DetectDetector(stream);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (detector == null)
            return;

        item.ChangedName = $"{item.ChangedName}.{detector.Extension}";
    }
}