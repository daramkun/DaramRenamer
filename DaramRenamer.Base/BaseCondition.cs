namespace DaramRenamer;

[Serializable]
public abstract class BaseCondition
{
    public abstract bool IsSatisfied(BaseFileInfo fileInfo);
}