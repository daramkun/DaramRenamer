using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Date
{
	[Serializable]
	public class DeleteDateProcessor : IProcessor
	{
		public string Name => "process_delete_date";
		public bool CannotMultithreadProcess => false;

		public bool Process ( FileInfo file )
		{
			string [] dateRegex = new [] {
				"[0-9][0-9][0-9][0-9][0-1][0-9][0-3][0-9]",
				"[0-9][0-9][0-9][0-9][0-3][0-9][0-1][0-9]",
				"[0-9][0-9][0-1][0-9][0-3][0-9]",
				"[0-9][0-9][0-3][0-9][0-1][0-9]",
				"[0-9]?[0-9]?[0-9][0-9]/[0-1]?[0-9]/[0-3]?[0-9]",
				"[0-9]?[0-9]?[0-9][0-9]/[0-3]?[0-9]/[0-1]?[0-9]",
				"[0-9][0-9]?[0-9][0-9]-[0-1]?[0-9]-[0-3]?[0-9]",
				"[0-9][0-9]?[0-9][0-9]-[0-3]?[0-9]-[0-1]?[0-9]",
			};

			foreach ( var regex in dateRegex )
			{
				var proceed = Regex.Replace ( file.ChangedFilename, regex, "" );
				if ( proceed != file.ChangedFilename )
				{
					file.ChangedFilename = proceed;
					break;
				}
			}

			return true;
		}
	}
}
