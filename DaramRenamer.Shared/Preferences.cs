using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Daramee.Blockar;

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
							_instance = BlockarObject.DeserializeFromJson(stream).ToObject<Preferences>();
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
		private bool hardwareAccelerated = RenderOptions.ProcessRenderMode == RenderMode.Default;

		[FieldOption(Name = "hw_accel_mode", IsRequired = false)]
		public bool HaredwareAccelerated
		{
			get => hardwareAccelerated;
			set
			{
				hardwareAccelerated = value;
				RenderOptions.ProcessRenderMode = value ? RenderMode.Default : RenderMode.SoftwareOnly;
			}
		}

		[FieldOption(Name = "rename_mode", IsRequired = false)]
		public RenameMode RenameMode { get; set; } = RenameMode.Move;
		internal int RenameModeInteger
		{
			get => (int)RenameMode;
			set => RenameMode = (RenameMode)value;
		}

		[FieldOption(Name = "auto_name_fix", IsRequired = false)]
		public bool AutomaticFixingFilename { get; set; } = true;
		[FieldOption(Name = "auto_list_clean", IsRequired = false)]
		public bool AutomaticListCleaning { get; set; } = false;
		[FieldOption(Name = "overwrite", IsRequired = false)]
		public bool Overwrite { get; set; } = false;

		[FieldOption(Name = "save_window_state", IsRequired = false)]
		public bool SaveWindowState { get; set; } = false;

		[FieldOption(Name = "left", IsRequired = false)]
		public double Left { get; set; }
		[FieldOption(Name = "top", IsRequired = false)]
		public double Top { get; set; }
		[FieldOption(Name = "width", IsRequired = false)]
		public double Width { get; set; }
		[FieldOption(Name = "height", IsRequired = false)]
		public double Height { get; set; }
		[FieldOption(Name = "window_state", IsRequired = false)]
		public WindowState WindowState { get; set; }

		[FieldOption(Name = "use_custom_plugins", IsRequired = false)]
		public bool UseCustomPlugins { get; set; } = false;

		[FieldOption(Name = "language", IsRequired = false)]
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
		private KeyBindingInfo[] _shortcuts = new KeyBindingInfo[10];

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

		[FieldOption(Name = "shortcut0", IsRequired = false)]
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

		[FieldOption(Name = "shortcut1", IsRequired = false)]
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

		[FieldOption(Name = "shortcut2", IsRequired = false)]
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

		[FieldOption(Name = "shortcut3", IsRequired = false)]
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

		[FieldOption(Name = "shortcut4", IsRequired = false)]
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

		[FieldOption(Name = "shortcut5", IsRequired = false)]
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

		[FieldOption(Name = "shortcut6", IsRequired = false)]
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

		[FieldOption(Name = "shortcut7", IsRequired = false)]
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

		[FieldOption(Name = "shortcut8", IsRequired = false)]
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

		[FieldOption(Name = "shortcut9", IsRequired = false)]
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

		public void ShortcutRebinding()
		{
			Shortcut0 = Shortcut0;
			Shortcut1 = Shortcut1;
			Shortcut2 = Shortcut2;
			Shortcut3 = Shortcut3;
			Shortcut4 = Shortcut4;
			Shortcut5 = Shortcut5;
			Shortcut6 = Shortcut6;
			Shortcut7 = Shortcut7;
			Shortcut8 = Shortcut8;
			Shortcut9 = Shortcut9;
		}

		public void Save()
		{
			using Stream stream = File.Open($"{AppDomain.CurrentDomain.BaseDirectory}\\DaramRenamer.config.json", FileMode.Create);
			BlockarObject.SerializeToJson(stream, BlockarObject.FromObject(this));
		}
	}
}
