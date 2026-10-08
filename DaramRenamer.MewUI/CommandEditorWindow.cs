using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using DaramRenamer.Registry;

namespace DaramRenamer.MewUI;

internal sealed class CommandEditorWindow : Window
{
    private readonly object _target;
    private readonly Action<bool> _completed;
    private readonly Action? _valueChanged;
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
                .Height(24)
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

        if (option.ValueType.IsEnum)
        {
            var values = Enum.GetValues(option.ValueType).Cast<object>().ToArray();
            var current = option.GetValue(_target);
            var selectedIndex = Array.IndexOf(values, current);
            var comboBox = new ComboBox()
                .Items(values, value => Strings.Instance[value.ToString() ?? string.Empty])
                .SelectedIndex(Math.Max(selectedIndex, 0));
            comboBox.OnSelectionChanged(value =>
            {
                if (value is null)
                    return;

                var index = Array.FindIndex(values, candidate => Equals(candidate, value));
                if (index < 0)
                    index = Array.FindIndex(values, candidate =>
                        string.Equals(candidate.ToString(), value.ToString(), StringComparison.Ordinal));
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

    private void Complete(bool accepted)
    {
        if (_didComplete)
            return;

        _didComplete = true;
        _completed(accepted);
        Close();
    }
}
