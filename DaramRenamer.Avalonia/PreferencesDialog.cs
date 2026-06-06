using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using DaramRenamer.Registry;

namespace DaramRenamer.Avalonia;

internal sealed class PreferencesDialog : Window
{
    private readonly ComboBox _renameMode = new();
    private readonly CheckBox _autoFix = new();
    private readonly CheckBox _autoClean = new();
    private readonly CheckBox _overwrite = new();
    private readonly CheckBox _closeApply = new();
    private readonly CheckBox _removeEmpty = new();
    private readonly CheckBox _disableUpdate = new();
    private readonly CheckBox _saveWindowState = new();
    private readonly TextBox _language = new();
    private readonly CheckBox _visualCommand = new();
    private readonly CheckBox _forceSingleCore = new();
    private readonly List<ShortcutEditor> _shortcutEditors = [];

    public PreferencesDialog()
    {
        var look = NativeTheme.Current;
        Title = Strings.Instance["Preferences"];
        Width = 520;
        SizeToContent = SizeToContent.Height;
        Background = look.AppBackground;

        _renameMode.ItemsSource = new[]
        {
            new RenameModeItem(RenameMode.Move),
            new RenameModeItem(RenameMode.Copy)
        };

        LoadValues();

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 6
        };
        var ok = new Button { Content = "OK", MinWidth = 76 };
        ok.Click += (_, _) =>
        {
            SaveValues();
            Close(true);
        };
        var cancel = new Button { Content = "Cancel", MinWidth = 76 };
        cancel.Click += (_, _) => Close(false);
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);

        var root = new StackPanel
        {
            Margin = look.IsMacOS ? new Thickness(18) : new Thickness(16),
            Spacing = look.IsMacOS ? 11 : 10,
        };
        root.Children.Add(new TextBlock
        {
            Text = Strings.Instance["PreferencesRenameMode"],
            Foreground = look.Text,
            FontWeight = FontWeight.SemiBold
        });
        root.Children.Add(_renameMode);
        root.Children.Add(_overwrite);
        root.Children.Add(_autoFix);
        root.Children.Add(_autoClean);
        root.Children.Add(_closeApply);
        root.Children.Add(_removeEmpty);
        root.Children.Add(_disableUpdate);
        root.Children.Add(_saveWindowState);
        root.Children.Add(_visualCommand);
        root.Children.Add(_forceSingleCore);
        root.Children.Add(new TextBlock
        {
            Text = Strings.Instance["PreferencesCurrentLanguage"],
            Foreground = look.Text,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 8, 0, 0)
        });
        root.Children.Add(_language);
        root.Children.Add(new TextBlock
        {
            Text = Strings.Instance["PreferencesTabShortcut"],
            Foreground = look.Text,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 8, 0, 0)
        });
        root.Children.Add(BuildShortcutEditors());
        root.Children.Add(buttons);

        Content = root;
    }

    private void LoadValues()
    {
        var prefs = AvaloniaPreferences.Instance;
        _renameMode.SelectedIndex = prefs.RenameMode == RenameMode.Copy ? 1 : 0;
        _overwrite.Content = Strings.Instance["PreferencesOverwrite"];
        _overwrite.IsChecked = prefs.Overwrite;
        _autoFix.Content = Strings.Instance["PreferencesAutoFixFilename"];
        _autoFix.IsChecked = prefs.AutomaticFixingFilename;
        _autoClean.Content = Strings.Instance["PreferencesAutoListCleaning"];
        _autoClean.IsChecked = prefs.AutomaticListCleaning;
        _closeApply.Content = Strings.Instance["PreferencesCloseApplyWindowWhenSuccessfullyDone"];
        _closeApply.IsChecked = prefs.CloseApplyWindowWhenSuccessfullyDone;
        _removeEmpty.Content = Strings.Instance["PreferencesRemoveEmptyDirectory"];
        _removeEmpty.IsChecked = prefs.RemoveEmptyDirectory;
        _disableUpdate.Content = Strings.Instance["PreferencesDisableCheckUpdate"];
        _disableUpdate.IsChecked = prefs.DisableCheckUpdate;
        _saveWindowState.Content = Strings.Instance["PreferencesSaveWindowState"];
        _saveWindowState.IsChecked = prefs.SaveWindowState;
        _language.Text = prefs.CurrentLanguage;
        _visualCommand.Content = Strings.Instance["PreferencesVisualCommand"];
        _visualCommand.IsChecked = prefs.VisualCommand;
        _forceSingleCore.Content = Strings.Instance["PreferencesForceSingleCoreRunning"];
        _forceSingleCore.IsChecked = prefs.ForceSingleCoreRunning;
    }

    private void SaveValues()
    {
        var prefs = AvaloniaPreferences.Instance;
        prefs.RenameMode = _renameMode.SelectedIndex == 1 ? RenameMode.Copy : RenameMode.Move;
        prefs.Overwrite = _overwrite.IsChecked == true;
        prefs.AutomaticFixingFilename = _autoFix.IsChecked == true;
        prefs.AutomaticListCleaning = _autoClean.IsChecked == true;
        prefs.CloseApplyWindowWhenSuccessfullyDone = _closeApply.IsChecked == true;
        prefs.RemoveEmptyDirectory = _removeEmpty.IsChecked == true;
        prefs.DisableCheckUpdate = _disableUpdate.IsChecked == true;
        prefs.SaveWindowState = _saveWindowState.IsChecked == true;
        prefs.CurrentLanguage = _language.Text ?? string.Empty;
        prefs.VisualCommand = _visualCommand.IsChecked == true;
        prefs.ForceSingleCoreRunning = _forceSingleCore.IsChecked == true;
        prefs.Shortcuts = _shortcutEditors.Select(editor => editor.ToShortcut()).ToArray();
        prefs.Save();
    }

    private Control BuildShortcutEditors()
    {
        var panel = new StackPanel { Spacing = 6 };
        var commandItems = DaramRenamerRegistry.Commands
            .Where(command => command.Category != CommandCategory.NoCategorized)
            .OrderBy(command => command.Category)
            .ThenBy(command => command.Order)
            .Select(command => new CommandItem(command))
            .ToArray();
        var shortcuts = AvaloniaPreferences.Instance.Shortcuts;
        for (var i = 0; i < 10; ++i)
        {
            var shortcut = i < shortcuts.Length ? shortcuts[i] : new AvaloniaShortcutInfo();
            var editor = new ShortcutEditor(shortcut, commandItems);
            _shortcutEditors.Add(editor);
            panel.Children.Add(editor.Control);
        }

        return new ScrollViewer
        {
            Height = 250,
            Content = panel
        };
    }

    private sealed record RenameModeItem(RenameMode Value)
    {
        public override string ToString() => Strings.Instance[Value.ToString()];
    }

    private sealed record CommandItem(CommandDescriptor Descriptor)
    {
        public override string ToString() => Strings.Instance[Descriptor.LocalizationKey];
    }

    private sealed class ShortcutEditor
    {
        private readonly TextBox _gesture = new() { IsReadOnly = true, MinWidth = 120 };
        private readonly ComboBox _command = new() { MinWidth = 220 };

        public ShortcutEditor(AvaloniaShortcutInfo shortcut, CommandItem[] commands)
        {
            _gesture.Text = shortcut.KeyBinding;
            _command.ItemsSource = commands;
            _command.SelectedItem = commands.FirstOrDefault(command => command.Descriptor.Id == shortcut.Command);
            _gesture.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Back || e.Key == Key.Delete)
                    _gesture.Text = string.Empty;
                else if (e.Key != Key.None && e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
                         e.Key != Key.LeftAlt && e.Key != Key.RightAlt &&
                         e.Key != Key.LeftShift && e.Key != Key.RightShift)
                    _gesture.Text = AvaloniaShortcutInfo.FromKeyEvent(e);
                e.Handled = true;
            };
            Control = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 8,
                Children =
                {
                    _gesture,
                    _command
                }
            };
            Grid.SetColumn(_command, 1);
        }

        public Control Control { get; }

        public AvaloniaShortcutInfo ToShortcut() => new()
        {
            KeyBinding = _gesture.Text ?? string.Empty,
            Command = _command.SelectedItem is CommandItem item ? item.Descriptor.Id : string.Empty
        };
    }
}
