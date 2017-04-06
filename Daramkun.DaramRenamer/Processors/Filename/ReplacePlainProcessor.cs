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
		public string Name => "process_replace_plain_text";
		public bool CannotMultithreadProcess => false;

		[Globalized ( "original_text", 0 )]
		public string Original { get; set; } = "";
		[Globalized ( "replace_text", 1 )]
		public string Replace { get; set; } = "";
		[Globalized ( "include_extension", 2 )]
		public bool IncludeExtensions { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			if ( Original == null || Original.Length == 0 ) return false;
			if ( Replace == null ) Replace = "";
			file.ChangedFilename = IncludeExtensions ? $"{ file.ChangedFilename.Replace ( Original, Replace ) }" :
				$"{ Path.GetFileNameWithoutExtension ( file.ChangedFilename ).Replace ( Original, Replace ) }{ Path.GetExtension ( file.ChangedFilename ) }";
			return true;
		}
	}
}
