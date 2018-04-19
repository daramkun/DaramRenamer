using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Conditions
{
	public class ExtensionCondition : ICondition
	{
		public string Name => "condition_extension";

		[Argument ( Name = "extension" )]
		public string Extension { get; set; } = "";

		public bool IsValid ( FileInfo file )
		{
			return Path.GetExtension ( file.ChangedFilename ) == Extension;
		}
	}
}
