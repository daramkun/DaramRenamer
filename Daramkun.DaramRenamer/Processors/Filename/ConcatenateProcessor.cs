using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class ConcatenateProcessor : IProcessor
	{
		public string Name { get { return "process_concatenate_text"; } }
		public bool CannotMultithreadProcess { get { return false; } }

		[Globalized ( "concat_text", 0 )]
		public string ConcatenateText { get; set; } = "";
		[Globalized ( "concat_pos", 1 )]
		public OnePointPosition ConcatenatePosition { get; set; } = OnePointPosition.EndPoint;
		[Globalized ( "include_extension", 2 )]
		public bool IncludeExtensions { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			if ( ConcatenateText == null || ConcatenateText.Length == 0 ) return false;
			string fn = !IncludeExtensions ? Path.GetFileNameWithoutExtension ( file.ChangedFilename ) : file.ChangedFilename;
			string ext = !IncludeExtensions ? Path.GetExtension ( file.ChangedFilename ) : "";
			file.ChangedFilename = ConcatenatePosition == OnePointPosition.StartPoint ? $"{ConcatenateText}{fn}{ext}" :
				( ConcatenatePosition == OnePointPosition.EndPoint ? $"{fn}{ConcatenateText}{ext}" :
				$"{ConcatenateText}{fn}{ConcatenateText}{ext}" );
			return true;
		}
	}
}
