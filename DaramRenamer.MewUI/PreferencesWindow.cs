using System.Globalization;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using DaramRenamer.Registry;

namespace DaramRenamer.MewUI;

internal sealed class PreferencesWindow : Window
{
    private static readonly CommandCategory[] CommandCategories =
    [
        CommandCategory.Filename,
        CommandCategory.Extension,
        CommandCategory.Path,
        CommandCategory.Number,
        CommandCategory.Date,
        CommandCategory.Tag,
        CommandCategory.Etc
    ];

    private readonly MewUIPreferences _preferences = MewUIPreferences.Instance;
    private readonly Dictionary<string, Command> _shortcutCommands = new(StringComparer.Ordinal);
    private readonly Dictionary<int, DropDownButton> _shortcutButtons = [];
    private bool _closePermanently;

    public PreferencesWindow()
    {
        this.Title(Strings.Instance["Preferences"]);
        this.Fixed(420, 340);
        this.StartCenterOwner();
        this.OnClosing(args =>
        {
            if (_closePermanently)
                return;

            args.Cancel = true;
            Hide();
        });
        this.OnClosed(() =>
        {
            Strings.Instance.Loaded -= OnStringsLoaded;
        });
        this.Content(BuildUi());
        Strings.Instance.Loaded += OnStringsLoaded;
    }

    public void CloseForShutdown()
    {
        _closePermanently = true;
        Close();
    }

    private void OnStringsLoaded(object? sender, EventArgs args)
    {
        this.Title(Strings.Instance["Preferences"]);
        this.Content(BuildUi());
    }

    private Element BuildUi()
    {
        var renameModes = Enum.GetValues<RenameMode>();
        var renameMode = new ComboBox()
            .Items(renameModes, value => Strings.Instance[value.ToString()])
            .SelectedIndex(Array.IndexOf(renameModes, _preferences.RenameMode));
        renameMode.OnSelectionChanged(value =>
        {
            if (value is RenameMode mode)
                _preferences.RenameMode = mode;
            else if (Enum.TryParse(value?.ToString(), out RenameMode parsed))
                _preferences.RenameMode = parsed;
        });

        var languages = Strings.Instance.AvailableLanguages.ToArray();
        var languageCombo = new ComboBox()
            .Items(languages, culture => culture.NativeName)
            .SelectedIndex(Math.Max(Array.FindIndex(languages,
                culture => string.Equals(culture.Name, _preferences.CurrentLanguage,
                    StringComparison.OrdinalIgnoreCase)), 0));
        languageCombo.OnSelectionChanged(value =>
        {
            if (value is not CultureInfo culture)
                return;

            _preferences.CurrentLanguage = culture.Name;
            CultureInfo.CurrentUICulture = culture;
            Strings.Instance.Load();
            this.Title(Strings.Instance["Preferences"]);
            this.Content(BuildUi());
        });

        var general = new ScrollViewer()
            .AutoVerticalScroll()
            .NoHorizontalScroll()
            .Content(new StackPanel()
                .Vertical()
                .Spacing(2)
                .Padding(8)
                .Children(
                    Check("PreferencesHardwareAcceleration", () => _preferences.HaredwareAccelerated,
                        value => _preferences.HaredwareAccelerated = value),
                    Check("PreferencesOverwrite", () => _preferences.Overwrite,
                        value => _preferences.Overwrite = value),
                    Check("PreferencesAutoFixFilename", () => _preferences.AutomaticFixingFilename,
                        value => _preferences.AutomaticFixingFilename = value),
                    Check("PreferencesAutoListCleaning", () => _preferences.AutomaticListCleaning,
                        value => _preferences.AutomaticListCleaning = value),
                    Field(Strings.Instance["PreferencesRenameMode"], renameMode),
                    Check("PreferencesSaveWindowState", () => _preferences.SaveWindowState,
                        value => _preferences.SaveWindowState = value),
                    Check("PreferencesUseCustomPlugins", () => _preferences.UseCustomPlugins,
                        value => _preferences.UseCustomPlugins = value),
                    Field(Strings.Instance["PreferencesCurrentLanguage"], languageCombo),
                    Check("PreferencesVisualCommand", () => _preferences.VisualCommand,
                        value => _preferences.VisualCommand = value),
                    Check("PreferencesForceSingleCoreRunning", () => _preferences.ForceSingleCoreRunning,
                        value => _preferences.ForceSingleCoreRunning = value),
                    Check("PreferencesCloseApplyWindowWhenSuccessfullyDone",
                        () => _preferences.CloseApplyWindowWhenSuccessfullyDone,
                        value => _preferences.CloseApplyWindowWhenSuccessfullyDone = value),
                    Check("PreferencesRemoveEmptyDirectory", () => _preferences.RemoveEmptyDirectory,
                        value => _preferences.RemoveEmptyDirectory = value),
                    Check("PreferencesDisableCheckUpdate", () => _preferences.DisableCheckUpdate,
                        value => _preferences.DisableCheckUpdate = value)));

        var shortcuts = BuildShortcutList();
        return new TabControl()
            .TabItems(
                new TabItem()
                    .Header(Strings.Instance["PreferencesTabGeneral"])
                    .Content(general),
                new TabItem()
                    .Header(Strings.Instance["PreferencesTabShortcut"])
                    .Content(shortcuts));
    }

    private Element BuildShortcutList()
    {
        var commands = CommandCategories
            .SelectMany(DaramRenamerRegistry.GetCommands)
            .OrderBy(descriptor => descriptor.Order)
            .ToArray();
        _shortcutButtons.Clear();

        var rows = _preferences.Shortcuts.Select((shortcut, index) =>
        {
            var keyBox = new TextBox()
                .Text(shortcut.KeyBinding)
                .IsReadOnly(true)
                .Margin(0, 0, 8, 0);
            keyBox.OnKeyDown(args => CaptureKeyBinding(shortcut, keyBox, args));

            var commandButton = new DropDownButton()
                .Content(ShortcutCommandText(commands, shortcut.Command))
                .DropDownMenu(BuildShortcutCommandMenu(index));
            _shortcutButtons[index] = commandButton;

            return new Grid()
                .Columns("64,120,*")
                .Padding(2)
                .Children(
                    new Label()
                        .Text(Strings.Instance["PreferencesTabShortcutShortcutText"])
                        .CenterVertical(),
                    keyBox.Column(1),
                    commandButton.Column(2));
        }).ToArray();

        return new ScrollViewer()
            .AutoVerticalScroll()
            .NoHorizontalScroll()
            .Content(new StackPanel()
                .Vertical()
                .Padding(6)
                .Children(rows));
    }

    private Menu BuildShortcutCommandMenu(int shortcutIndex)
    {
        var categories = new Menu();
        foreach (var category in CommandCategories)
        {
            var commands = new Menu();
            foreach (var descriptor in DaramRenamerRegistry.GetCommands(category))
            {
                var itemDescriptor = descriptor;
                var label = Strings.Instance[itemDescriptor.LocalizationKey];
                commands.Items.Add(new MenuItem(label).Command(GetShortcutCommand(shortcutIndex, itemDescriptor)));
            }

            categories.SubMenu(Strings.Instance[CategoryLocalizationKey(category)], commands);
        }

        var menu = new Menu();
        menu.SubMenu(Strings.Instance["Menu_Command_Command"], categories);
        menu.Items.Add(new MenuItem(Strings.Instance["Menu_Condition_Condition"]).IsEnabled(false));
        return menu;
    }

    private Command GetShortcutCommand(int shortcutIndex, ItemDescriptor descriptor)
    {
        var key = $"{shortcutIndex}:{descriptor.Id}";
        var label = Strings.Instance[descriptor.LocalizationKey];
        if (_shortcutCommands.TryGetValue(key, out var existing))
        {
            existing.Presentation.AccessText = label;
            return existing;
        }

        var command = new Command($"DaramRenamer.preferences.shortcut.{key}", label);
        this.Commands.Register(command, () =>
        {
            _preferences.Shortcuts[shortcutIndex].Command = descriptor.Id;
            if (_shortcutButtons.TryGetValue(shortcutIndex, out var button))
                button.Content(Strings.Instance[descriptor.LocalizationKey]);
        });
        _shortcutCommands.Add(key, command);
        return command;
    }

    private static string ShortcutCommandText(IEnumerable<ItemDescriptor> commands, string? commandId)
    {
        var descriptor = commands.FirstOrDefault(item =>
            string.Equals(item.Id, commandId, StringComparison.Ordinal));
        return descriptor is null ? string.Empty : Strings.Instance[descriptor.LocalizationKey];
    }

    private static string CategoryLocalizationKey(CommandCategory category) => category switch
    {
        CommandCategory.Filename => "Menu_Command_Filename",
        CommandCategory.Extension => "Menu_Command_Extension",
        CommandCategory.Path => "Menu_Command_Path",
        CommandCategory.Number => "Menu_Command_Number",
        CommandCategory.Date => "Menu_Command_Date",
        CommandCategory.Tag => "Menu_Command_Tag",
        _ => "Menu_Command_Etc"
    };

    private static void CaptureKeyBinding(ShortcutSetting shortcut, TextBox textBox, KeyEventArgs args)
    {
        var keyName = args.Key.ToString();
        var parts = new List<string>();
        if ((args.Modifiers & ModifierKeys.Control) != 0)
            parts.Add("Ctrl");
        if ((args.Modifiers & ModifierKeys.Alt) != 0)
            parts.Add("Alt");
        if ((args.Modifiers & ModifierKeys.Shift) != 0)
            parts.Add("Shift");
        parts.Add(keyName);

        shortcut.KeyBinding = string.Join("+", parts);
        textBox.Text(shortcut.KeyBinding);
        args.Handled = true;
    }

    private static CheckBox Check(string key, Func<bool> getValue, Action<bool> setValue)
    {
        var checkBox = new CheckBox()
            .Content(Strings.Instance[key])
            .IsChecked(getValue());
        checkBox.OnCheckedChanged(setValue);
        return checkBox;
    }

    private static Grid Field(string label, Element editor) => new Grid()
        .Columns("Auto,*")
        .Spacing(8)
        .Children(
            new Label().Text(label).CenterVertical(),
            editor.Column(1));
}
