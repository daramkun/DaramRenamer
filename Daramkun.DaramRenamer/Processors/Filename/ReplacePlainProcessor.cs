using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class ReplacePlainProcessor : IProcessor
	{
		[Globalized( "original_text" )]
		public string Original { get; set; }
		[Globalized ( "replace_text" )]
		public string Replace { get; set; }
		[Globalized( "include_extension" )]
		public bool IncludeExtensions { get; set; }

		public ReplacePlainProcessor ( string original, string replace, bool includeExtensions = false )
		{
			Original = original; Replace = replace;
			IncludeExtensions = includeExtensions;
		}

		public bool Process ( FileInfo file )
		{
			if ( Original == null || Original.Length == 0 ) return false;
			if ( Replace == null ) Replace = "";
			file.ChangedFilename = IncludeExtensions ? $"{ file.ChangedFilename.Replace ( Original, Replace ) }" :
				$"{ Path.GetFileNameWithoutExtension ( file.ChangedFilename ).Replace ( Original, Replace ) }{ Path.GetExtension ( file.ChangedFilename ) }";
			return true;
		}

		public override string ToString ()
		{
			if ( CultureInfo.CurrentUICulture == CultureInfo.GetCultureInfo ( "ko-KR" ) ) return "일반 텍스트 파일명 치환";
			return "Replace Plain Text Filename";
		}
	}
}
