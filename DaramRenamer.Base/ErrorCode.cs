namespace DaramRenamer;

public enum ErrorCode
{
    NoError,
    Unknown,
    FileNotFound,
    DirectoryNotFound,
    PathTooLong,
    IOError,
    UnauthorizedAccess,
    FailedOverwrite
}
