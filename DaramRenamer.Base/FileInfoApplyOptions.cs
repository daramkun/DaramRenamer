namespace DaramRenamer;

public enum RenameMode
{
    Move,
    Copy,
}

[Serializable]
public class FileInfoApplyOptions(bool autoFix, RenameMode renameMode, bool overwrite)
{
    public bool AutoFixInvalidCharacters => autoFix;
    public RenameMode RenameMode => renameMode;
    public bool Overwrite => overwrite;
}