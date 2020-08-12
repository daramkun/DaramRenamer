using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
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

		[FieldOption (Name = "hw_accel_mode", IsRequired = false)]
		public bool HaredwareAccelerated
		{
			get => RenderOptions.ProcessRenderMode == RenderMode.Default;
			set => RenderOptions.ProcessRenderMode = value ? RenderMode.Default : RenderMode.SoftwareOnly;
		}

		[FieldOption (Name = "rename_mode", IsRequired = false)]
		public RenameMode RenameMode { get; set; } = RenameMode.Move;
		internal int RenameModeInteger
		{
			get => (int) RenameMode;
			set => RenameMode = (RenameMode) value;
		}

		[FieldOption (Name = "auto_name_fix", IsRequired = false)]
		public bool AutomaticFixingFilename { get; set; } = true;
		[FieldOption (Name = "auto_list_clean", IsRequired = false)]
		public bool AutomaticListCleaning { get; set; } = false;
		[FieldOption (Name = "overwrite", IsRequired = false)]
		public bool Overwrite { get; set; } = false;

		[FieldOption (Name = "save_window_state", IsRequired = false)]
		public bool SaveWindowState { get; set; } = false;

		[FieldOption (Name = "left", IsRequired = false)]
		public double Left { get; set; }
		[FieldOption (Name = "top", IsRequired = false)]
		public double Top { get; set; }
		[FieldOption (Name = "width", IsRequired = false)]
		public double Width { get; set; }
		[FieldOption (Name = "height", IsRequired = false)]
		public double Height { get; set; }
		[FieldOption (Name = "window_state", IsRequired = false)]
		public WindowState WindowState { get; set; }

		[FieldOption (Name = "use_custom_plugins", IsRequired = false)]
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

		public void Save()
		{
			using Stream stream = File.Open ($"{AppDomain.CurrentDomain.BaseDirectory}\\DaramRenamer.config.json", FileMode.Create);
			BlockarObject.SerializeToJson(stream, BlockarObject.FromObject(this));
		}
	}
}
