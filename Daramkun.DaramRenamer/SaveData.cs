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
	}
}
