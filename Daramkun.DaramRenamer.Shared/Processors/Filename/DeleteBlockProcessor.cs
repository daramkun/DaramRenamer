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
	public class DeleteBlockProcessor : IProcessor
	{
		public string Name => "process_delete_block";
		public bool CannotMultithreadProcess => false;
		
		[Argument ( Name = "start_block" )]
		public string StartText { get; set; } = "";
		[Argument ( Name = "end_block" )]
		public string EndText { get; set; } = "";
		[Argument ( Name = "delete_all_blocks" )]
		public bool DeleteAllBlocks { get; set; } = false;
		[Argument ( Name = "include_extension" )]
		public bool IncludeExtensions { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			if ( StartText == null || StartText.Length == 0 ) return false;
			if ( EndText == null || EndText.Length == 0 ) return false;
			string fn = !IncludeExtensions ? Path.GetFileNameWithoutExtension ( file.ChangedFilename ) : file.ChangedFilename;
			string ext = !IncludeExtensions ? Path.GetExtension ( file.ChangedFilename ) : "";
			
			int first, last;
			while ( ( first = fn.IndexOf ( StartText ) ) != -1 )
			{
				last = fn.IndexOf ( EndText, first + 1 );
				if ( last == -1 ) break;
				fn = fn.Remove ( first, last - first + EndText.Length );
				if ( !DeleteAllBlocks ) break;
			}

			file.ChangedFilename = $"{fn}{ext}";

			return true;
		}
	}
}
