namespace DaramRenamer
{
	public interface ICondition
	{
		bool IsSatisfyThisCondition(FileInfo file);
	}
}
