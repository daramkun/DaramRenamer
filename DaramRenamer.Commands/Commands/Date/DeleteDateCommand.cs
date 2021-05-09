using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DaramRenamer.Commands.Date
{
	[Serializable, LocalizationKey("Command_Name_DeleteDate")]
	public class DeleteDateDateCommand : ICommand, IOrderBy
	{
		private static readonly Regex[] DateRegexps = new[]
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

		public int Order => int.MinValue + 1;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Date;

		public bool DoCommand(FileInfo file)
		{
			foreach (var regex in DateRegexps)
			{
				var proceed = regex.Replace(file.ChangedFilename, "");
				if (proceed == file.ChangedFilename)
					continue;
				file.ChangedFilename = proceed;
				break;
			}

			return true;
		}
	}
}
