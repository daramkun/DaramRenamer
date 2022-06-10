namespace DaramRenamer;

public interface ICommand
{
    bool ParallelProcessable { get; }
    CommandCategory Category { get; }
    bool DoCommand(FileInfo file);
}