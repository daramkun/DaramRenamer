namespace DaramRenamer.Registry;

public abstract class ItemDescriptor
{
    protected ItemDescriptor(
        string id,
        string localizationKey,
        Type itemType,
        Func<object> factory,
        IReadOnlyList<IOptionDescriptor> options,
        params string[] legacyIds)
    {
        Id = id;
        LocalizationKey = localizationKey;
        ItemType = itemType;
        Factory = factory;
        Options = options;
        LegacyIds = legacyIds;
    }

    public string Id { get; }
    public string LocalizationKey { get; }
    public Type ItemType { get; }
    public Func<object> Factory { get; }
    public IReadOnlyList<IOptionDescriptor> Options { get; }
    public IReadOnlyList<string> LegacyIds { get; }

    public object Create() => Factory();

    public object Clone(object source)
    {
        var target = Create();
        foreach (var option in Options)
            option.SetValue(target, option.GetValue(source));
        return target;
    }

    public bool MatchesId(string id) =>
        Id == id || ItemType.FullName == id || LegacyIds.Contains(id);
}

public sealed class CommandDescriptor : ItemDescriptor
{
    public CommandDescriptor(
        string id,
        string localizationKey,
        CommandCategory category,
        int order,
        Func<ICommand> factory,
        IReadOnlyList<IOptionDescriptor> options,
        params string[] legacyIds)
        : base(id, localizationKey, factory().GetType(), factory, options, legacyIds)
    {
        Category = category;
        Order = order;
    }

    public CommandCategory Category { get; }
    public int Order { get; }

    public new ICommand Create() => (ICommand)base.Create();
    public ICommand Clone(ICommand source) => (ICommand)base.Clone(source);
}

public sealed class ConditionDescriptor : ItemDescriptor
{
    public ConditionDescriptor(
        string id,
        string localizationKey,
        int order,
        Func<ICondition> factory,
        IReadOnlyList<IOptionDescriptor> options,
        params string[] legacyIds)
        : base(id, localizationKey, factory().GetType(), factory, options, legacyIds)
    {
        Order = order;
    }

    public int Order { get; }

    public new ICondition Create() => (ICondition)base.Create();
    public ICondition Clone(ICondition source) => (ICondition)base.Clone(source);
}
