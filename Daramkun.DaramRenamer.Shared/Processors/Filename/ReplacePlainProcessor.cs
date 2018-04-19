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
	public class ReplacePlainProcessor : IProcessor
	{
		public string Name => "process_replace_plain_text";
		public bool CannotMultithreadProcess => false;
		
		[Argument ( Name = "original_text" )]
		public string Original { get; set; } = "";
		[Argument ( Name = "replace_text" )]
		public string Replace { get; set; } = "";
		[Argument ( Name = "include_extension" )]
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
