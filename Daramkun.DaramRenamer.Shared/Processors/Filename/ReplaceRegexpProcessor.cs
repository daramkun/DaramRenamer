using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	[Serializable]
	public class ReplaceRegexpProcessor : IProcessor
	{
		public string Name => "process_replace_regex_text";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "original_regex" )]
		public Regex RegularExpression { get; set; } = new Regex ( "$^" );
		[Argument ( Name = "replace_text" )]
		public string ReplaceText { get; set; } = "";
		[Argument ( Name = "include_extension" )]
		public bool IncludeExtensions { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			string filename = !IncludeExtensions ? Path.GetFileNameWithoutExtension ( file.ChangedFilename ) : file.ChangedFilename;
			string ext = !IncludeExtensions ? Path.GetExtension ( file.ChangedFilename ) : "";
			file.ChangedFilename = $"{RegularExpression.Replace ( filename, ReplaceText )}{ext}";
			return true;
		}
	}
}
