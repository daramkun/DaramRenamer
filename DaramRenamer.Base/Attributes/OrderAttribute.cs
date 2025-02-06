namespace DaramRenamer.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class OrderAttribute(int order = int.MinValue) : Attribute
{
    public int Order { get; } = order;
}