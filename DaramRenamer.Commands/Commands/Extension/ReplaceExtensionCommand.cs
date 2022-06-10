using System;
using System.IO;

namespace DaramRenamer.Commands.Extension;

[Serializable]
[LocalizationKey("Command_Name_ReplaceExtension")]
public class ReplaceExtensionCommand : ICommand
{
    public int Order => int.MinValue + 3;

    [LocalizationKey("Command_Argument_ReplaceExtension_Extension")]
    public string Extension { get; set; } = string.Empty;

    [LocalizationKey("Commamd_Argument_ReplaceExtension_ApplyToDirectory")]
    public bool ApplyToDirectory { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Extension;

    public bool DoCommand(FileInfo file)
    {
        if (string.IsNullOrEmpty(Extension))
            return false;

        if (!ApplyToDirectory && file.IsDirectory)
            return true;

        file.ChangedFilename =
            $"{Path.GetFileNameWithoutExtension(file.ChangedFilename)}{(Extension[0] != '.' ? "." : "")}{Extension}";
        return true;
    }
}