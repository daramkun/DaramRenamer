using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	[Serializable]
	public class ConcatenateProcessor : IProcessor
	{
		public string Name => "process_concatenate_text";
		public bool CannotMultithreadProcess => false;
		
		[Argument ( Name = "concat_text" )]
		public string Text { get; set; } = "";
		[Argument ( Name = "concat_pos" )]
		public OnePointPosition Position { get; set; } = OnePointPosition.StartPoint;
		[Argument ( Name = "include_extension" )]
		public bool IncludeExtensions { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			if ( Text == null || Text.Length == 0 ) return false;
			string fn = !IncludeExtensions ? Path.GetFileNameWithoutExtension ( file.ChangedFilename ) : file.ChangedFilename;
			string ext = !IncludeExtensions ? Path.GetExtension ( file.ChangedFilename ) : "";
			file.ChangedFilename = Position == OnePointPosition.StartPoint ? $"{Text}{fn}{ext}" :
				( Position == OnePointPosition.EndPoint ? $"{fn}{Text}{ext}" :
				$"{Text}{fn}{Text}{ext}" );
			return true;
		}
	}
}
