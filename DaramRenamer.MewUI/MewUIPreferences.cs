using System.Globalization;
using System.Text;
using System.Text.Json;
using Aprillz.MewUI;

namespace DaramRenamer.MewUI;

internal sealed class MewUIPreferences
{
    private static readonly string ConfigPath = GetConfigPath();

    private static MewUIPreferences? _instance;

    public static MewUIPreferences Instance
    {
        get
        {
            if (_instance is not null)
                return _instance;

            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = NormalizeLegacySettings(File.ReadAllText(ConfigPath, Encoding.UTF8));
                    _instance = JsonSerializer.Deserialize<MewUIPreferences>(json);
                }

                if (_instance is not null && !string.IsNullOrWhiteSpace(_instance.CurrentLanguage))
                {
                    CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(_instance.CurrentLanguage);
                    Strings.Instance.Load();
                }
            }
            catch
            {
                // Use defaults when a damaged or older MewUI settings file cannot be loaded.
            }

            _instance ??= new MewUIPreferences();
            _instance.NormalizeShortcuts();
            if (!Enum.IsDefined(_instance.Theme))
                _instance.Theme = ThemeVariant.System;
            return _instance;
        }
    }

    public RenameMode RenameMode { get; set; } = RenameMode.Move;
    public bool AutomaticFixingFilename { get; set; } = true;
    public bool AutomaticListCleaning { get; set; }
    public bool Overwrite { get; set; }
    public bool CloseApplyWindowWhenSuccessfullyDone { get; set; } = true;
    public bool RemoveEmptyDirectory { get; set; }
    public bool VisualCommand { get; set; }
    public bool ForceSingleCoreRunning { get; set; }
    public bool DisableCheckUpdate { get; set; }
    public bool SaveWindowState { get; set; }
    public bool UseCustomPlugins { get; set; }
    public bool HaredwareAccelerated { get; set; } = true;
    public ThemeVariant Theme { get; set; } = ThemeVariant.System;
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int WindowState { get; set; }
    public string CurrentLanguage { get; set; } = CultureInfo.CurrentUICulture.Name;
    public ShortcutSetting[] Shortcuts { get; set; } = Enumerable.Range(0, 10)
        .Select(_ => new ShortcutSetting())
        .ToArray();

    public ShortcutSetting Shortcut0 { get => GetShortcut(0); set => SetShortcut(0, value); }
    public ShortcutSetting Shortcut1 { get => GetShortcut(1); set => SetShortcut(1, value); }
    public ShortcutSetting Shortcut2 { get => GetShortcut(2); set => SetShortcut(2, value); }
    public ShortcutSetting Shortcut3 { get => GetShortcut(3); set => SetShortcut(3, value); }
    public ShortcutSetting Shortcut4 { get => GetShortcut(4); set => SetShortcut(4, value); }
    public ShortcutSetting Shortcut5 { get => GetShortcut(5); set => SetShortcut(5, value); }
    public ShortcutSetting Shortcut6 { get => GetShortcut(6); set => SetShortcut(6, value); }
    public ShortcutSetting Shortcut7 { get => GetShortcut(7); set => SetShortcut(7, value); }
    public ShortcutSetting Shortcut8 { get => GetShortcut(8); set => SetShortcut(8, value); }
    public ShortcutSetting Shortcut9 { get => GetShortcut(9); set => SetShortcut(9, value); }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        }), Encoding.UTF8);
    }

    private ShortcutSetting GetShortcut(int index) =>
        index < Shortcuts.Length ? Shortcuts[index] : new ShortcutSetting();

    private void SetShortcut(int index, ShortcutSetting shortcut)
    {
        NormalizeShortcuts();
        Shortcuts[index] = shortcut ?? new ShortcutSetting();
    }

    private void NormalizeShortcuts()
    {
        if (Shortcuts is not { Length: >= 10 })
        {
            var normalized = Enumerable.Range(0, 10).Select(_ => new ShortcutSetting()).ToArray();
            if (Shortcuts is not null)
                Array.Copy(Shortcuts, normalized, Math.Min(Shortcuts.Length, normalized.Length));
            Shortcuts = normalized;
        }

        for (var index = 0; index < 10; ++index)
        {
            Shortcuts[index] ??= new ShortcutSetting();
            Shortcuts[index].KeyBinding ??= string.Empty;
            Shortcuts[index].Command ??= string.Empty;
        }
    }

    private static string NormalizeLegacySettings(string json)
    {
        // Keep accepting the legacy WPF configuration file, including the old snake_case names.
        // WPF also used the misspelled HaredwareAccelerated property in its current JSON format.
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["hw_accel_mode"] = "HaredwareAccelerated",
            ["HardwareAccelerated"] = "HaredwareAccelerated",
            ["rename_mode"] = "RenameMode",
            ["auto_name_fix"] = "AutomaticFixingFilename",
            ["auto_list_clean"] = "AutomaticListCleaning",
            ["overwrite"] = "Overwrite",
            ["save_window_state"] = "SaveWindowState",
            ["left"] = "Left",
            ["top"] = "Top",
            ["width"] = "Width",
            ["height"] = "Height",
            ["window_state"] = "WindowState",
            ["use_custom_plugins"] = "UseCustomPlugins",
            ["language"] = "CurrentLanguage"
        };

        foreach (var (legacyName, currentName) in aliases)
            json = json.Replace($"\"{legacyName}\"", $"\"{currentName}\"", StringComparison.Ordinal);

        return json;
    }

    private static string GetConfigPath()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var isReadOnly = false;
        try
        {
            var attributes = File.GetAttributes(baseDirectory);
            isReadOnly = attributes.HasFlag(FileAttributes.ReadOnly) || attributes.HasFlag(FileAttributes.System);
        }
        catch
        {
            isReadOnly = true;
        }

        if (!isReadOnly)
        {
            try
            {
                using var probe = File.Create(Path.Combine(baseDirectory, Path.GetRandomFileName()), 1,
                    FileOptions.DeleteOnClose);
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                isReadOnly = true;
            }
        }

        if (!isReadOnly)
            return Path.Combine(baseDirectory, "DaramRenamer.config.json");

        var localDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var installationKey = OperatingSystem.IsWindows()
            ? baseDirectory.Replace(":", string.Empty)
            : Path.GetFileName(Path.TrimEndingDirectorySeparator(baseDirectory));
        var fallbackDirectory = Path.Combine(localDirectory, "DaramRenamer", installationKey);
        Directory.CreateDirectory(fallbackDirectory);
        return Path.Combine(fallbackDirectory, "DaramRenamer.config.json");
    }
}

internal sealed class ShortcutSetting
{
    public string KeyBinding { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}
