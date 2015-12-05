using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class DeleteBlockProcessor : IProcessor
	{
		public string StartText { get; set; }
		public string EndText { get; set; }
		public bool DeleteAllBlock { get; set; }
		public bool IncludeExtensions { get; set; }

		public DeleteBlockProcessor(string start, string end, bool deleteAllBlock, bool includeExtensions = false )
		{
			StartText = start;EndText = end;
			DeleteAllBlock = deleteAllBlock;
			IncludeExtensions = includeExtensions;
		}

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
				if ( !DeleteAllBlock ) break;
			}

			file.ChangedFilename = $"{fn}{ext}";

			return true;
		}

		public override string ToString ()
		{
			if ( CultureInfo.CurrentUICulture == CultureInfo.GetCultureInfo ( "ko-KR" ) ) return "파일명 블록 제거";
			return "Delete Block of filename";
		}
	}
}
