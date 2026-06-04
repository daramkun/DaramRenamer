using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

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

        Content = new StackPanel
        {
            Margin = look.IsMacOS ? new Thickness(18) : new Thickness(16),
            Spacing = look.IsMacOS ? 11 : 10,
            Children =
            {
                new TextBlock
                {
                    Text = Strings.Instance["PreferencesRenameMode"],
                    Foreground = look.Text,
                    FontWeight = FontWeight.SemiBold
                },
                _renameMode,
                _overwrite,
                _autoFix,
                _autoClean,
                _closeApply,
                _removeEmpty,
                _disableUpdate,
                _saveWindowState,
                buttons
            }
        };
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
        prefs.Save();
    }

    private sealed record RenameModeItem(RenameMode Value)
    {
        public override string ToString() => Strings.Instance[Value.ToString()];
    }
}
