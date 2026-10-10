using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using DaramRenamer.Registry;

namespace DaramRenamer.MewUI;

internal sealed class CommandEditorWindow : Window
{
    private const double OptionRowHeight = 24;
    private const double RadioButtonSpacing = 2;

    private readonly object _target;
    private readonly Action<bool> _completed;
    private readonly Action? _valueChanged;
    private readonly string _enumRadioGroupId = Guid.NewGuid().ToString("N");
    private TextBox? _firstTextBox;
    private bool _didComplete;

    public CommandEditorWindow(
        object target,
        ItemDescriptor descriptor,
        Action<bool> completed,
        Action? valueChanged = null)
    {
        _target = target;
        _completed = completed;
        _valueChanged = valueChanged;

        this.Title(Strings.Instance[descriptor.LocalizationKey]);
        this.FitContentHeight(480, 480);
        this.ShowInTaskbar(false);
        this.StartCenterOwner();
        this.OnPreviewKeyDown(args =>
        {
            if (args.Key == Key.Enter)
            {
                Complete(true);
                args.Handled = true;
            }
            else if (args.Key == Key.Escape)
            {
                Complete(false);
                args.Handled = true;
            }
        });
        this.OnClosed(() =>
        {
            if (_didComplete)
                return;

            _didComplete = true;
            _completed(false);
        });
        this.Content(BuildUi(descriptor))
            .OnLoaded(() => _firstTextBox?.Focus());
    }

    private Grid BuildUi(ItemDescriptor descriptor)
    {
        var fields = descriptor.Options.Select(option =>
        {
            var editor = BuildEditor(option);
            return new Grid()
                .Columns("120,*")
                .Height(OptionRowHeight)
                .Children(
                    new Label()
                        .Text(Strings.Instance[option.LocalizationKey])
                        .CenterVertical(),
                    editor.Column(1));
        }).ToArray();

        var content = new StackPanel()
            .Vertical()
            .Padding(15)
            .Children(fields);

        var buttons = new Border()
            .BorderThickness(new Thickness(0, 1, 0, 0))
            .BorderBrush(new Color(227, 227, 227))
            .Background(new Color(240, 240, 240))
            .WithTheme((theme, border) =>
            {
                border.BorderBrush(theme.IsDark ? new Color(64, 64, 64) : new Color(227, 227, 227));
                border.Background(theme.IsDark ? new Color(40, 40, 40) : new Color(240, 240, 240));
            })
            .Child(new StackPanel()
                .Horizontal()
                .Spacing(5)
                .Right()
                .Padding(0, 9, 20, 6)
                .Children(
                    new Button()
                        .Content(Strings.Instance["ButtonOK"])
                        .Width(72)
                        .Height(22)
                        .OnClick(() => Complete(true)),
                    new Button()
                        .Content(Strings.Instance["ButtonCancel"])
                        .Width(72)
                        .Height(22)
                        .OnClick(() => Complete(false))));

        return new Grid()
            .Rows("*,Auto")
            .Children(
                content.Row(0),
                buttons.Row(1));
    }

    private Element BuildEditor(IOptionDescriptor option)
    {
        if (option.ValueKind is OptionValueKind.Boolean or OptionValueKind.NullableBoolean)
        {
            var checkBox = new CheckBox()
                .IsChecked(option.GetValue(_target) as bool?);
            if (option.ValueKind == OptionValueKind.NullableBoolean)
                checkBox.ThreeState();

            checkBox.OnCheckStateChanged(value =>
            {
                option.SetValue(_target, value);
                _valueChanged?.Invoke();
            });
            return checkBox;
        }

        var enumType = Nullable.GetUnderlyingType(option.ValueType) ?? option.ValueType;
        if (enumType.IsEnum)
        {
            var values = Enum.GetValues(enumType).Cast<object>().ToArray();
            var current = option.GetValue(_target);
            var selectedIndex = Array.IndexOf(values, current);

            if (UsesRadioButtons(enumType, values.Length))
            {
                var radioButtons = values.Select(value =>
                {
                    var radioButton = new RadioButton()
                        .Content(GetEnumDisplayName(value))
                        .GroupName($"{_enumRadioGroupId}.{option.Id}")
                        .IsChecked(Equals(value, current));
                    radioButton.OnCheckedChanged(isChecked =>
                    {
                        if (!isChecked)
                            return;

                        option.SetValue(_target, value);
                        _valueChanged?.Invoke();
                    });
                    return radioButton;
                }).ToArray();

                return new StackPanel()
                    .Horizontal()
                    .Spacing(RadioButtonSpacing)
                    .Children(radioButtons);
            }

            var comboBox = new ComboBox()
                .Items(values, GetEnumDisplayName)
                .SelectedIndex(Math.Max(selectedIndex, 0));
            comboBox.OnSelectionChanged(value =>
            {
                if (value is null)
                    return;

                var index = Array.FindIndex(values, candidate => Equals(candidate, value));
                if (index < 0)
                {
                    var selectedText = value.ToString();
                    index = Array.FindIndex(values, candidate =>
                        string.Equals(GetEnumDisplayName(candidate), selectedText, StringComparison.Ordinal) ||
                        string.Equals(candidate.ToString(), selectedText, StringComparison.Ordinal));
                }

                if (index < 0)
                    return;

                option.SetValue(_target, values[index]);
                _valueChanged?.Invoke();
            });
            return comboBox;
        }

        var textBox = new TextBox().Text(option.SerializeValue(_target));
        _firstTextBox ??= textBox;
        textBox.OnTextChanged(value =>
        {
            try
            {
                option.DeserializeValue(_target, value);
                _valueChanged?.Invoke();
            }
            catch
            {
                // Leave temporarily invalid input in the editor until it can be parsed.
            }
        });
        return textBox;
    }

    private static bool UsesRadioButtons(Type enumType, int valueCount) =>
        valueCount is > 0 and < 3 || enumType.Name.StartsWith("Position", StringComparison.Ordinal);

    private static string GetEnumDisplayName(object value)
    {
        var enumName = Enum.GetName(value.GetType(), value) ?? value.ToString() ?? string.Empty;
        return Strings.Instance[enumName];
    }

    private void Complete(bool accepted)
    {
        if (_didComplete)
            return;

        _didComplete = true;
        _completed(accepted);
        Close();
    }
}