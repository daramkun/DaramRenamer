using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Filename
{
	public class ReplaceRegexpProcessor : IProcessor
	{
		public string Name => "process_replace_regex_text";
		public bool CannotMultithreadProcess => false;

		[Globalized ( "original_regex", 0 )]
		public Regex RegularExpression { get; set; } = new Regex ( "$^" );
		[Globalized ( "replace_format", 1 )]
		public string FormatString { get; set; } = "";
		[Globalized ( "include_extension", 2 )]
		public bool IncludeExtensions { get; set; } = false;

		public bool Process ( FileInfo file )
		{
			try
			{
				string ext = !IncludeExtensions ? Path.GetExtension ( file.ChangedFilename ) : "";
				Match match = RegularExpression.Match ( IncludeExtensions ? file.ChangedFilename :
					Path.GetFileNameWithoutExtension ( file.ChangedFilename ) );
				GroupCollection group = match.Groups;
				object [] groupArr = new object [ group.Count ];
				for ( int i = 0; i < groupArr.Length; i++ )
					groupArr [ i ] = group [ i ].Value.Trim ();
				file.ChangedFilename = $"{string.Format ( FormatString, groupArr )}{ext}";
			}
			catch { return false; }
			return true;
		}
	}
}
