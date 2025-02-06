namespace DaramRenamer.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CommandCategoryAttribute(CommandCategory category) : Attribute
{
    public CommandCategory Category { get; } = category;
}