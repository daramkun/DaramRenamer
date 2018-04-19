using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Conditions
{
	public class RegexpCondition : ICondition
	{
		public string Name => "condition_regexp";

		[Argument ( Name = "regexp" )]
		public Regex RegularExpression { get; set; }

		public bool IsValid ( FileInfo file )
		{
			return RegularExpression.IsMatch ( file.ChangedFilename );
		}
	}
}
