using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace DaramRenamer.Avalonia;

internal sealed class AvaloniaPreferences
{
    private static readonly string BaseDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DaramRenamer");

    private static readonly string FilePath = Path.Combine(BaseDirectory, "DaramRenamer.Avalonia.config.json");
    private static AvaloniaPreferences? _instance;

    public static AvaloniaPreferences Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath, Encoding.UTF8);
                    _instance = JsonSerializer.Deserialize<AvaloniaPreferences>(json) ?? new AvaloniaPreferences();
                    _instance.Normalize();
                }
            }
            catch
            {
                _instance = new AvaloniaPreferences();
            }

            _instance ??= new AvaloniaPreferences();
            _instance.Normalize();
            return _instance;
        }
    }

    public RenameMode RenameMode { get; set; } = RenameMode.Move;
    public bool AutomaticFixingFilename { get; set; } = true;
    public bool AutomaticListCleaning { get; set; }
    public bool Overwrite { get; set; }
    public bool CloseApplyWindowWhenSuccessfullyDone { get; set; } = true;
    public bool RemoveEmptyDirectory { get; set; }
    public bool DisableCheckUpdate { get; set; }
    public string CurrentLanguage { get; set; } = CultureInfo.CurrentUICulture.ToString();
    public bool VisualCommand { get; set; }
    public bool ForceSingleCoreRunning { get; set; }
    public AvaloniaShortcutInfo[] Shortcuts { get; set; } =
    [
        new(), new(), new(), new(), new(),
        new(), new(), new(), new(), new()
    ];
    public bool SaveWindowState { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; } = 960;
    public double Height { get; set; } = 640;

    public void Save()
    {
        Normalize();
        if (!string.IsNullOrWhiteSpace(CurrentLanguage))
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(CurrentLanguage);
            Strings.Instance.Load();
        }

        Directory.CreateDirectory(BaseDirectory);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json, Encoding.UTF8);
    }

    private void Normalize()
    {
        CurrentLanguage = string.IsNullOrWhiteSpace(CurrentLanguage)
            ? CultureInfo.CurrentUICulture.ToString()
            : CurrentLanguage;

        var shortcuts = Shortcuts ?? [];
        if (shortcuts.Length < 10)
            shortcuts = shortcuts.Concat(Enumerable.Range(0, 10 - shortcuts.Length).Select(_ => new AvaloniaShortcutInfo())).ToArray();
        Shortcuts = shortcuts.Take(10).Select(shortcut => shortcut ?? new AvaloniaShortcutInfo()).ToArray();
    }
}
