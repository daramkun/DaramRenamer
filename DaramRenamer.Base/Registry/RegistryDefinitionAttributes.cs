namespace DaramRenamer.Registry;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CommandDefinitionAttribute(
    Type type,
    string id,
    string localizationKey,
    params string[] legacyIds) : Attribute
{
    public Type Type { get; } = type;
    public string Id { get; } = id;
    public string LocalizationKey { get; } = localizationKey;
    public string[] LegacyIds { get; } = legacyIds;
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ConditionDefinitionAttribute(
    Type type,
    string id,
    string localizationKey,
    params string[] legacyIds) : Attribute
{
    public Type Type { get; } = type;
    public string Id { get; } = id;
    public string LocalizationKey { get; } = localizationKey;
    public string[] LegacyIds { get; } = legacyIds;
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class OptionDefinitionAttribute(
    Type ownerType,
    string propertyName,
    string localizationKey) : Attribute
{
    public Type OwnerType { get; } = ownerType;
    public string PropertyName { get; } = propertyName;
    public string LocalizationKey { get; } = localizationKey;
}
