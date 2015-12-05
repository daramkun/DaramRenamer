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
		public string ConcatenateText { get; set; }
		public Position ConcatenatePosition { get; set; }
		public bool IncludeExtensions { get; set; }

		public ConcatenateProcessor ( string concat, Position pos, bool includeExtensions = false )
		{
			ConcatenateText = concat; ConcatenatePosition = pos;
			IncludeExtensions = includeExtensions;
		}

		public bool Process ( FileInfo file )
		{
			if ( ConcatenateText == null || ConcatenateText.Length == 0 ) return false;
			string fn = !IncludeExtensions ? Path.GetFileNameWithoutExtension ( file.ChangedFilename ) : file.ChangedFilename;
			string ext = !IncludeExtensions ? Path.GetExtension ( file.ChangedFilename ) : "";
			file.ChangedFilename = ConcatenatePosition == Position.StartPoint ? $"{ConcatenateText}{fn}{ext}" :
				( ConcatenatePosition == Position.EndPoint ? $"{fn}{ConcatenateText}{ext}" :
				$"{ConcatenateText}{fn}{ConcatenateText}{ext}" );
			return true;
		}

		public override string ToString ()
		{
			if ( CultureInfo.CurrentUICulture == CultureInfo.GetCultureInfo ( "ko-KR" ) ) return "파일명에 텍스트 잇기";
			return "Concatenate Text to Filename";
		}
	}
}
