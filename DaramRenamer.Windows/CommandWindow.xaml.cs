using System;
using System.Windows;
using System.Windows.Controls;
using DaramRenamer.Registry;

namespace DaramRenamer;

public partial class CommandWindow : Window
{
    private readonly object _target;
    private readonly ItemDescriptor _descriptor;

    public CommandWindow(ICommand command)
    {
        InitializeComponent();
        Command = command;
        _target = command;
        _descriptor = DaramRenamerRegistry.GetDescriptor(command)
                      ?? throw new InvalidOperationException($"Unregistered command: {command.GetType().FullName}");

        Title = Strings.Instance[_descriptor.LocalizationKey];
        Initialize();
    }

    public CommandWindow(ICondition condition)
    {
        InitializeComponent();
        Condition = condition;
        _target = condition;
        _descriptor = DaramRenamerRegistry.GetDescriptor(condition)
                      ?? throw new InvalidOperationException($"Unregistered condition: {condition.GetType().FullName}");

        Title = Strings.Instance[_descriptor.LocalizationKey];
        Initialize();
    }

    public ICommand Command { get; }
    public ICondition Condition { get; }

    public event EventHandler ValueChanged;

    private void Initialize()
    {
        for (var i = 0; i < _descriptor.Options.Count; ++i)
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });

        TextBox firstTextBox = null;

        for (var row = 0; row < _descriptor.Options.Count; ++row)
        {
            var option = _descriptor.Options[row];
            var textBlock = new TextBlock
            {
                Text = Strings.Instance[option.LocalizationKey],
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, 0);
            contentGrid.Children.Add(textBlock);

            var control = CreateControl(option, ref firstTextBox);
            Grid.SetRow(control, row);
            Grid.SetColumn(control, 1);
            contentGrid.Children.Add(control);
        }

        firstTextBox?.Focus();
    }

    private FrameworkElement CreateControl(IOptionDescriptor option, ref TextBox firstTextBox)
    {
        if (option.ValueKind == OptionValueKind.Boolean || option.ValueKind == OptionValueKind.NullableBoolean)
        {
            var checkBox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                IsThreeState = option.ValueKind == OptionValueKind.NullableBoolean,
                IsChecked = option.GetValue(_target) as bool?
            };
            checkBox.Checked += (sender, e) =>
            {
                option.SetValue(_target, true);
                OnValueChanged(sender, e);
            };
            checkBox.Unchecked += (sender, e) =>
            {
                option.SetValue(_target, option.ValueKind == OptionValueKind.NullableBoolean ? null : false);
                OnValueChanged(sender, e);
            };
            return checkBox;
        }

        if (option.ValueKind == OptionValueKind.Enum)
        {
            var comboBox = new ComboBox { VerticalAlignment = VerticalAlignment.Center };
            var values = Enum.GetValues(option.ValueType);
            foreach (var value in values)
                comboBox.Items.Add(Strings.Instance[value?.ToString() ?? string.Empty]);
            comboBox.SelectedItem = Strings.Instance[option.GetValue(_target)?.ToString() ?? string.Empty];
            comboBox.SelectionChanged += (sender, e) =>
            {
                option.SetValue(_target, values.GetValue(comboBox.SelectedIndex));
                OnValueChanged(sender, e);
            };
            return comboBox;
        }

        var textBox = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = option.SerializeValue(_target)
        };
        textBox.TextChanged += (sender, e) =>
        {
            try
            {
                option.DeserializeValue(_target, textBox.Text);
                OnValueChanged(sender, e);
            }
            catch
            {
                // Keep the user's text while it is temporarily invalid.
            }
        };
        firstTextBox ??= textBox;
        return textBox;
    }

    private void ButtonOK_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void OnValueChanged(object sender, RoutedEventArgs e)
    {
        ValueChanged?.Invoke(sender, e);
    }
}
