namespace DaramRenamer;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class LocalizationKeyAttribute(string key) : Attribute
{
    public string LocalizationKey { get; } = key;
}
