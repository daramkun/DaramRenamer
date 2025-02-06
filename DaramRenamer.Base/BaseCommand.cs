namespace DaramRenamer;

[Serializable]
public abstract class BaseCommand
{
    public virtual bool IsParallelizable => true;
    public virtual int Order => 0;

    public string? CommandName => GetType().FullName;
    public virtual string? LegacyCommandName => null;
    
    public abstract CommandCategory Category { get; }

    public abstract bool DoCommand(BaseFileInfo fileInfo);
}