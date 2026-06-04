using System.Globalization;

namespace DaramRenamer.Registry;

public sealed class OptionDescriptor<TTarget, TValue> : IOptionDescriptor
{
    private readonly Func<TTarget, TValue> _get;
    private readonly Action<TTarget, TValue> _set;

    public OptionDescriptor(
        string id,
        string localizationKey,
        OptionValueKind valueKind,
        Func<TTarget, TValue> get,
        Action<TTarget, TValue> set)
    {
        Id = id;
        Name = id;
        LocalizationKey = localizationKey;
        ValueKind = valueKind;
        ValueType = typeof(TValue);
        _get = get;
        _set = set;
    }

    public string Id { get; }
    public string Name { get; }
    public string LocalizationKey { get; }
    public OptionValueKind ValueKind { get; }
    public Type ValueType { get; }

    public object? GetValue(object target) => _get((TTarget)target);

    public void SetValue(object target, object? value)
    {
        if (value is null)
        {
            _set((TTarget)target, default!);
            return;
        }

        _set((TTarget)target, (TValue)value);
    }

    public string SerializeValue(object target)
    {
        var value = GetValue(target);
        return value switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    public void DeserializeValue(object target, string value)
    {
        SetValue(target, Parse(value));
    }

    private object? Parse(string value)
    {
        if (string.IsNullOrEmpty(value) && Nullable.GetUnderlyingType(ValueType) != null)
            return null;

        var type = Nullable.GetUnderlyingType(ValueType) ?? ValueType;

        if (type == typeof(string))
            return value;
        if (type == typeof(bool))
            return bool.Parse(value);
        if (type == typeof(int))
            return int.Parse(value, CultureInfo.InvariantCulture);
        if (type == typeof(uint))
            return uint.Parse(value, CultureInfo.InvariantCulture);
        if (type.IsEnum)
            return Enum.Parse(type, value, false);

        throw new NotSupportedException($"Unsupported option type: {ValueType.FullName}");
    }
}

public interface IOptionDescriptor
{
    string Id { get; }
    string Name { get; }
    string LocalizationKey { get; }
    OptionValueKind ValueKind { get; }
    Type ValueType { get; }
    object? GetValue(object target);
    void SetValue(object target, object? value);
    string SerializeValue(object target);
    void DeserializeValue(object target, string value);
}
