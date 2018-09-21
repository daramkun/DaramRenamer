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

		[DataMember ( IsRequired = false, Name = "save_window_state" )]
		public bool SaveWindowState { get; set; } = false;

		[DataMember ( IsRequired = false, Name = "left" )]
		public double Left { get; set; }
		[DataMember ( IsRequired = false, Name = "top" )]
		public double Top { get; set; }
		[DataMember ( IsRequired = false, Name = "width" )]
		public double Width { get; set; }
		[DataMember ( IsRequired = false, Name = "height" )]
		public double Height { get; set; }
		[DataMember ( IsRequired = false, Name = "window_state" )]
		public System.Windows.WindowState WindowState { get; set; }

		[DataMember ( IsRequired = false, Name = "toolbar_icon_pack" )]
		public string ToolBarIconPack { get; set; } = null;
	}
}
