namespace DaramRenamer.Registry;

public static partial class DaramRenamerRegistry
{
    public static IEnumerable<CommandDescriptor> GetCommands(CommandCategory category) =>
        Commands.Where(command => command.Category == category).OrderBy(command => command.Order);

    public static CommandDescriptor? FindCommandDescriptor(string? id) =>
        id == null
            ? null
            : Commands.FirstOrDefault(command => command.MatchesId(id) || command.LocalizationKey == id);

    public static ConditionDescriptor? FindConditionDescriptor(string? id) =>
        id == null
            ? null
            : Conditions.FirstOrDefault(condition => condition.MatchesId(id) || condition.LocalizationKey == id);

    public static CommandDescriptor? GetDescriptor(ICommand command) =>
        Commands.FirstOrDefault(descriptor => descriptor.ItemType == command.GetType());

    public static ConditionDescriptor? GetDescriptor(ICondition condition) =>
        Conditions.FirstOrDefault(descriptor => descriptor.ItemType == condition.GetType());

    public static ItemDescriptor? GetDescriptor(object item) =>
        item switch
        {
            ICommand command => GetDescriptor(command),
            ICondition condition => GetDescriptor(condition),
            _ => null
        };

    private static CommandDescriptor Command<T>(
        string id,
        string localizationKey,
        CommandCategory category,
        int order,
        IReadOnlyList<IOptionDescriptor> options,
        params string[] legacyIds)
        where T : ICommand, new() =>
        new(id, localizationKey, category, order, static () => new T(), options, legacyIds);

    private static ConditionDescriptor Condition<T>(
        string id,
        string localizationKey,
        int order,
        IReadOnlyList<IOptionDescriptor> options,
        params string[] legacyIds)
        where T : ICondition, new() =>
        new(id, localizationKey, order, static () => new T(), options, legacyIds);

    private static OptionDescriptor<TTarget, TValue> Option<TTarget, TValue>(
        string id,
        string localizationKey,
        OptionValueKind valueKind,
        Func<TTarget, TValue> get,
        Action<TTarget, TValue> set) =>
        new(id, localizationKey, valueKind, get, set);
}
