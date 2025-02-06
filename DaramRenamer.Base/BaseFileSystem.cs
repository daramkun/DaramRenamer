namespace DaramRenamer;

public abstract class BaseFileSystem
{
    public abstract IEnumerable<string> EnumerateFiles(string directory, bool topDirectoryOnly);
    
    public abstract bool IsFileExists(string path);
    public abstract bool IsDirectoryExists(string path);

    public abstract Stream OpenRead(string path);
    
    public abstract BaseFileOperator OpenFileOperator();
}

public abstract class BaseFileOperator(BaseFileSystem fileSystem) : IDisposable
{
    private bool _isDisposed = false;
    
    public abstract bool IsTransactable { get; }
    
    public BaseFileSystem FileSystem => fileSystem;

    ~BaseFileOperator() => Dispose(false);

    public void Dispose()
    {
        AssertIfDisposed();
        
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _isDisposed = true;
    }
    
    public abstract void Move(string destination, string source, bool overwrite);
    public abstract void Copy(string destination, string source, bool overwrite);

    protected void AssertIfDisposed()
    {
        if (!_isDisposed) return;
        throw new ObjectDisposedException(GetType().Name);
    }
}