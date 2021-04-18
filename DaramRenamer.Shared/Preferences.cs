using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace DaramRenamer
{
	public class Preferences
	{
		private static Preferences _instance;

		public static Preferences Instance
		{
			get
			{
				switch (_instance)
				{
					case null when File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\DaramRenamer.config.json"):
						{
							using Stream stream =
								File.Open($"{AppDomain.CurrentDomain.BaseDirectory}\\DaramRenamer.config.json",
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
							_instance = JsonSerializer.Deserialize<Preferences>(jsonBytes);
							break;
						}
					case null:
						_instance = new Preferences();
						break;
				}

				return _instance;
			}
		}

		[NonSerialized]
		private bool _hardwareAccelerated = RenderOptions.ProcessRenderMode == RenderMode.Default;
		
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
			get => (int)RenameMode;
			set => RenameMode = (RenameMode)value;
		}
		
		public bool AutomaticFixingFilename { get; set; } = true;
		public bool AutomaticListCleaning { get; set; } = false;
		public bool Overwrite { get; set; } = false;
		
		public bool SaveWindowState { get; set; } = false;
		
		public double Left { get; set; }
		public double Top { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public WindowState WindowState { get; set; }
		
		public bool UseCustomPlugins { get; set; } = false;
		
		public string CurrentLanguage
		{
			get => CultureInfo.CurrentUICulture.ToString();
			set
			{
				CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(value);
				Strings.Instance.Load();
			}
		}

		[NonSerialized]
		private readonly KeyBindingInfo[] _shortcuts = new KeyBindingInfo[10];

		public KeyBindingInfo[] Shortcuts => _shortcuts;

		private static InputGesture StringToKeyGesture(string k)
		{
			if (k == null)
				return null;

			var keys = k.Split('+');

			ModifierKeys modifierKeys = ModifierKeys.None;
			Key key = Key.None;

			foreach (var keyString in keys)
			{
				if (keyString == "Ctrl")
					modifierKeys |= ModifierKeys.Control;
				else if (keyString == "Alt")
					modifierKeys |= ModifierKeys.Alt;
				else if (keyString == "Shift")
					modifierKeys |= ModifierKeys.Shift;
				else
					key = Enum.Parse<Key>(keyString);
			}

			return new KeyGesture(key, modifierKeys);
		}
		
		public KeyBindingInfo Shortcut0
		{
			get => _shortcuts[0];
			set
			{
				_shortcuts[0] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 0].Gesture = gesture;
			}
		}
		
		public KeyBindingInfo Shortcut1
		{
			get => _shortcuts[1];
			set
			{
				_shortcuts[1] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 1].Gesture = gesture;
			}
		}
		
		public KeyBindingInfo Shortcut2
		{
			get => _shortcuts[2];
			set
			{
				_shortcuts[2] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 2].Gesture = gesture;
			}
		}
		
		public KeyBindingInfo Shortcut3
		{
			get => _shortcuts[3];
			set
			{
				_shortcuts[3] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 3].Gesture = gesture;
			}
		}
		
		public KeyBindingInfo Shortcut4
		{
			get => _shortcuts[4];
			set
			{
				_shortcuts[4] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 4].Gesture = gesture;
			}
		}
		
		public KeyBindingInfo Shortcut5
		{
			get => _shortcuts[5];
			set
			{
				_shortcuts[5] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 5].Gesture = gesture;
			}
		}
		
		public KeyBindingInfo Shortcut6
		{
			get => _shortcuts[6];
			set
			{
				_shortcuts[6] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 6].Gesture = gesture;
			}
		}

		public KeyBindingInfo Shortcut7
		{
			get => _shortcuts[7];
			set
			{
				_shortcuts[7] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 7].Gesture = gesture;
			}
		}
		
		public KeyBindingInfo Shortcut8
		{
			get => _shortcuts[8];
			set
			{
				_shortcuts[8] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 8].Gesture = gesture;
			}
		}
		
		public KeyBindingInfo Shortcut9
		{
			get => _shortcuts[9];
			set
			{
				_shortcuts[9] = value;
				var window = Application.Current?.MainWindow as MainWindow;
				var gesture = StringToKeyGesture(value.KeyBinding);
				if (window != null && gesture != null)
					window.InputBindings[8 + 9].Gesture = gesture;
			}
		}

		public bool VisualCommand { get; set; } = false;
		public bool ForceSingleCoreRunning { get; set; } = false;
		public bool CloseApplyWindowWhenSuccessfullyDone { get; set; } = true;

		public void Save()
		{
			using Stream stream = File.Open($"{AppDomain.CurrentDomain.BaseDirectory}\\DaramRenamer.config.json", FileMode.Create);
			var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(this, typeof(Preferences), new JsonSerializerOptions()
			{
				WriteIndented = false, AllowTrailingCommas = false,
			});
			stream.Write(jsonBytes, 0, jsonBytes.Length);
		}
	}
}
