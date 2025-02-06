using System.Reflection;
using Daramee.FileTypeDetector;
using DaramRenamer.Attributes;

namespace DaramRenamer.Commands;

[Serializable]
[LocalizationKey("Command_Name_AddExtensionAuto")]
public sealed class AddExtensionAutoCommand : BaseCommand
{
    public override CommandCategory Category => CommandCategory.Extension;
    public override int Order => int.MinValue + 1;
    
    public AddExtensionAutoCommand()
    {
        DetectorService.AddDetectors(Assembly.Load(new AssemblyName("DaramRenamer.Commands")));
    }

    public override bool DoCommand(BaseFileInfo file)
    {
        if (file.IsDirectory)
            return true;

        if (!File.Exists(file.OriginalFullPath))
            return false;

        using Stream stream = File.OpenRead(file.OriginalFullPath);
        var detector = DetectorService.DetectDetector(stream);

        if (detector == null)
            return false;

        file.ChangedName = $"{file.ChangedName}.{detector.Extension}";

        return true;
    }
}