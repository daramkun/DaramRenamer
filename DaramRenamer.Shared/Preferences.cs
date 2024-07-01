using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace DaramRenamer;

public class Preferences
{
    private static Preferences _instance;

    private static readonly string BaseDirectory = GetBaseDirectory();

    [NonSerialized]
    private bool _hardwareAccelerated = RenderOptions.ProcessRenderMode == RenderMode.Default;

    public static Preferences Instance
    {
        get
        {
            switch (_instance)
            {
                case null when File.Exists($"{BaseDirectory}\\DaramRenamer.config.json"):
                {
                    using Stream stream =
                        File.Open($"{BaseDirectory}\\DaramRenamer.config.json",
                            FileMode.Open);
                    using TextReader reader = new StreamReader(stream, Encoding.UTF8, true, -1, true);
                    var jsonString = reader.ReadToEnd();

                    // Conversion from Legacy Settings file
                    if (jsonString.Contains("\"hw_accel_mode\"") || jsonString.Contains("\"rename_mode\"") ||
                        jsonString.Contains("\"auto_name_fix\"") || jsonString.Contains("\"auto_list_clean\"") ||
                        jsonString.Contains("\"overwrite\"") || jsonString.Contains("\"save_window_state\"") ||
                        jsonString.Contains("\"left\"") || jsonString.Contains("\"top\"") ||
                        jsonString.Contains("\"width\"") || jsonString.Contains("\"height\"") ||
                        jsonString.Contains("\"window_state\"") || jsonString.Contains("\"use_custom_plugins\"") ||
                        jsonString.Contains("\"language\""))
                    {
                        jsonString = jsonString.Replace("\"hw_accel_mode\"", "\"HardwareAccelerated\"");
                        jsonString = jsonString.Replace("\"rename_mode\"", "\"RenameMode\"");
                        jsonString = jsonString.Replace("\"auto_name_fix\"", "\"AutomaticFixingFilename\"");
                        jsonString = jsonString.Replace("\"auto_list_clean\"", "\"AutomaticListCleaning\"");
                        jsonString = jsonString.Replace("\"overwrite\"", "\"Overwrite\"");
                        jsonString = jsonString.Replace("\"save_window_state\"", "\"SaveWindowState\"");
                        jsonString = jsonString.Replace("\"left\"", "\"Left\"");
                        jsonString = jsonString.Replace("\"top\"", "\"Top\"");
                        jsonString = jsonString.Replace("\"width\"", "\"Width\"");
                        jsonString = jsonString.Replace("\"height\"", "\"Height\"");
                        jsonString = jsonString.Replace("\"window_state\"", "\"WindowState\"");
                        jsonString = jsonString.Replace("\"use_custom_plugins\"", "\"UseCustomPlugins\"");
                        jsonString = jsonString.Replace("\"language\"", "\"Language\"");
                    }

                    var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                    _instance = JsonSerializer.Deserialize<Preferences>(jsonBytes) ?? new Preferences();

                    break;
                }
                case null:
                    _instance = new Preferences();
                    break;
            }

            return _instance;
        }
    }

    public bool HaredwareAccelerated
    {
        get => _hardwareAccelerated;
        set
        {
            _hardwareAccelerated = value;
            RenderOptions.ProcessRenderMode = value ? RenderMode.Default : RenderMode.SoftwareOnly;
        }
    }

    public RenameMode RenameMode { get; set; } = RenameMode.Move;

    internal int RenameModeInteger
    {
        get => (int) RenameMode;
        set => RenameMode = (RenameMode) value;
    }

    public bool AutomaticFixingFilename { get; set; } = true;
    public bool AutomaticListCleaning { get; set; }
    public bool Overwrite { get; set; }

    public bool SaveWindowState { get; set; }

    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public WindowState WindowState { get; set; }

    public bool UseCustomPlugins { get; set; }

    public string CurrentLanguage
    {
        get => CultureInfo.CurrentUICulture.ToString();
        set
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(value);
            Strings.Instance.Load();
        }
    }

    [field: NonSerialized]
    public KeyBindingInfo[] Shortcuts { get; } =
    {
        new(),
        new(),
        new(),
        new(),
        new(),
        new(),
        new(),
        new(),
        new(),
        new()
    };

    public KeyBindingInfo Shortcut0
    {
        get => Shortcuts[0];
        set
        {
            Shortcuts[0] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 0].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut1
    {
        get => Shortcuts[1];
        set
        {
            Shortcuts[1] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 1].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut2
    {
        get => Shortcuts[2];
        set
        {
            Shortcuts[2] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 2].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut3
    {
        get => Shortcuts[3];
        set
        {
            Shortcuts[3] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 3].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut4
    {
        get => Shortcuts[4];
        set
        {
            Shortcuts[4] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 4].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut5
    {
        get => Shortcuts[5];
        set
        {
            Shortcuts[5] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 5].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut6
    {
        get => Shortcuts[6];
        set
        {
            Shortcuts[6] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 6].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut7
    {
        get => Shortcuts[7];
        set
        {
            Shortcuts[7] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 7].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut8
    {
        get => Shortcuts[8];
        set
        {
            Shortcuts[8] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 8].Gesture = gesture;
        }
    }

    public KeyBindingInfo Shortcut9
    {
        get => Shortcuts[9];
        set
        {
            Shortcuts[9] = value;
            var window = Application.Current?.MainWindow as MainWindow;
            var gesture = StringToKeyGesture(value.KeyBinding);
            if (window != null && gesture != null)
                window.InputBindings[8 + 9].Gesture = gesture;
        }
    }

    public bool VisualCommand { get; set; }
    public bool ForceSingleCoreRunning { get; set; }
    public bool CloseApplyWindowWhenSuccessfullyDone { get; set; } = true;
    public bool RemoveEmptyDirectory { get; set; }
    public bool DisableCheckUpdate { get; set; }

    private static string GetBaseDirectory()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var attr = File.GetAttributes(baseDir);
        var isReadOnly = attr.HasFlag(FileAttributes.ReadOnly) || attr.HasFlag(FileAttributes.System);

        if (!isReadOnly)
            try
            {
                using var fs = File.Create(Path.Combine(baseDir, Path.GetRandomFileName()), 1,
                    FileOptions.DeleteOnClose);
            }
            catch (UnauthorizedAccessException ex)
            {
                isReadOnly = true;
            }

        if (isReadOnly)
        {
            var localDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            baseDir = Path.Combine(localDir, "DaramRenamer", baseDir.Replace(":", ""));
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
        }

        return baseDir;
    }

    private static InputGesture StringToKeyGesture(string k)
    {
        if (k == null)
            return null;

        var keys = k.Split('+');

        var modifierKeys = ModifierKeys.None;
        var key = Key.None;

        foreach (var keyString in keys)
            if (keyString == "Ctrl")
                modifierKeys |= ModifierKeys.Control;
            else if (keyString == "Alt")
                modifierKeys |= ModifierKeys.Alt;
            else if (keyString == "Shift")
                modifierKeys |= ModifierKeys.Shift;
            else
                key = Enum.Parse<Key>(keyString);

        return new KeyGesture(key, modifierKeys);
    }

    public void Save()
    {
        using Stream stream = File.Open($"{BaseDirectory}\\DaramRenamer.config.json", FileMode.Create);
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(this, typeof(Preferences), new JsonSerializerOptions
        {
            WriteIndented = true,
            AllowTrailingCommas = false
        });
        stream.Write(jsonBytes, 0, jsonBytes.Length);
    }
}