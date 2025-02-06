namespace DaramRenamer.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class LocalizationKeyAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}