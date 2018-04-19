using Daramee.Nargs;
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
	[Serializable]
	public class RearrangeRegexpProcessor : IProcessor
	{
		public string Name => "process_rearrange_regex_text";
		public bool CannotMultithreadProcess => false;

		[Argument ( Name = "original_regex" )]
		public Regex RegularExpression { get; set; } = new Regex ( "$^" );
		[Argument ( Name = "rearrange_format" )]
		public string FormatString { get; set; } = "";
		[Argument ( Name = "include_extension" )]
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
