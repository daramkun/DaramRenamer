using System;
using System.IO;
using System.Text;
using System.Text.Json;

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
                }
            }
            catch
            {
                _instance = new AvaloniaPreferences();
            }

            return _instance ??= new AvaloniaPreferences();
        }
    }

    public RenameMode RenameMode { get; set; } = RenameMode.Move;
    public bool AutomaticFixingFilename { get; set; } = true;
    public bool AutomaticListCleaning { get; set; }
    public bool Overwrite { get; set; }
    public bool CloseApplyWindowWhenSuccessfullyDone { get; set; } = true;
    public bool RemoveEmptyDirectory { get; set; }
    public bool DisableCheckUpdate { get; set; }
    public bool SaveWindowState { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; } = 960;
    public double Height { get; set; } = 640;

    public void Save()
    {
        Directory.CreateDirectory(BaseDirectory);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json, Encoding.UTF8);
    }
}
