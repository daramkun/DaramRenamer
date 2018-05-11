using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;

namespace Daramkun.DaramRenamer
{
	public enum RenameMode : byte { Move, Copy }

	[DataContract]
	class SaveData
	{
		public RenameMode RenameMode { get; set; } = RenameMode.Move;
		[DataMember ( IsRequired = false, Name = "rename_mode" )]
		internal int RenameModeInteger { get { return ( int ) RenameMode; } set { RenameMode = ( RenameMode ) value; } }
		private bool hwAccelMode = true;
		[DataMember ( IsRequired = false, Name = "hw_accel_mode" )]
		public bool HardwareAccelerationMode
		{
			get { return hwAccelMode; }
			set
			{
				hwAccelMode = value;
				if ( hwAccelMode )
					RenderOptions.ProcessRenderMode = RenderMode.Default;
				else
					RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
			}
		}
		[DataMember ( IsRequired = false, Name = "auto_name_fix" )]
		public bool AutomaticFilenameFix { get; set; } = false;
		[DataMember ( IsRequired = false, Name = "auto_list_clean" )]
		public bool AutomaticListCleaning { get; set; } = false;
		[DataMember ( IsRequired = false, Name = "overwrite" )]
		public bool Overwrite { get; set; } = false;

		[DataMember ( IsRequired = false, Name = "left" )]
		public double Left
		{
			get { return App.Current.MainWindow.Left; }
			set { App.Current.MainWindow.Left = value; }
		}
		[DataMember ( IsRequired = false, Name = "top" )]
		public double Top
		{
			get { return App.Current.MainWindow.Top; }
			set { App.Current.MainWindow.Top = value; }
		}
		[DataMember ( IsRequired = false, Name = "width" )]
		public double Width
		{
			get { return App.Current.MainWindow.Width; }
			set { App.Current.MainWindow.Width = value; }
		}
		[DataMember ( IsRequired = false, Name = "height" )]
		public double Height
		{
			get { return App.Current.MainWindow.Height; }
			set { App.Current.MainWindow.Height = value; }
		}
		[DataMember ( IsRequired = false, Name = "window_state" )]
		public System.Windows.WindowState WindowState
		{
			get { return App.Current.MainWindow.WindowState; }
			set { App.Current.MainWindow.WindowState = value; }
		}
	}
}
