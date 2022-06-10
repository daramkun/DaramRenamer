using System;

namespace DaramRenamer.Commands.Extension;

[Serializable]
[LocalizationKey("Command_Name_AddExtension")]
public class AddExtensionCommand : ICommand, IOrderBy
{
    [LocalizationKey("Command_Argument_AddExtension_Extension")]
    public string Extension { get; set; } = string.Empty;

    [LocalizationKey("Commamd_Argument_AddExtension_ApplyToDirectory")]
    public bool ApplyToDirectory { get; set; } = false;

    public bool ParallelProcessable => true;
    public CommandCategory Category => CommandCategory.Extension;

    public bool DoCommand(FileInfo file)
    {
        if (string.IsNullOrEmpty(Extension))
            return false;

        if (!ApplyToDirectory && file.IsDirectory)
            return true;

        file.ChangedFilename = $"{file.ChangedFilename}{(Extension[0] != '.' ? "." : "")}{Extension}";
        return true;
    }

    public int Order => int.MinValue;
}