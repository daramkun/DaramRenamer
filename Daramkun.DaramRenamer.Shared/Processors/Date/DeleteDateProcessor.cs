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
		static Regex [] dateRegexes = new []
		{
			new Regex ( "[0-9][0-9][0-9][0-9][0-1][0-9][0-3][0-9]" ),
			new Regex ( "[0-9][0-9][0-9][0-9][0-3][0-9][0-1][0-9]" ),
			new Regex ( "[0-9][0-9][0-1][0-9][0-3][0-9]" ),
			new Regex ( "[0-9][0-9][0-3][0-9][0-1][0-9]" ),
			new Regex ( "[0-9]?[0-9]?[0-9][0-9]/[0-1]?[0-9]/[0-3]?[0-9]" ),
			new Regex ( "[0-9]?[0-9]?[0-9][0-9]/[0-3]?[0-9]/[0-1]?[0-9]" ),
			new Regex ( "[0-9][0-9]?[0-9][0-9]-[0-1]?[0-9]-[0-3]?[0-9]" ),
			new Regex ( "[0-9][0-9]?[0-9][0-9]-[0-3]?[0-9]-[0-1]?[0-9]" ),
			new Regex ( "((Sun)|(Mon)|(Tue)|(Wed)|(Thu)|(Fri)|(Sat)), [0-9][0-9] ((Jan)|(Fab)|(Mar)|(Apr)|(May)|(Jun)|(Jul)|(Aug)|(Sep)|(Oct)|(Nov)|(Dec)) [0-9][0-9][0-9][0-9] [0-2][0-9]:[0-6][0-9]:[0-6][0-9] [A-Z][A-Z][A-Z]" ),
		};

		public string Name => "process_delete_date";
		public bool CannotMultithreadProcess => false;

		public bool Process ( FileInfo file )
		{
			foreach ( var regex in dateRegexes )
			{
				var proceed = regex.Replace ( file.ChangedFilename, "" );
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
