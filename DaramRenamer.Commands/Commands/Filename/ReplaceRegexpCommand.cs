using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DaramRenamer.Commands.Filename
{
	[Serializable, LocalizationKey("Command_Name_ReplaceRegexp")]
	public class ReplaceRegexpCommand : ICommand, IOrderBy
	{
		public int Order => int.MinValue + 1;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Filename;

		[LocalizationKey("Command_Argument_ReplaceRegexp_Find")]
		public Regex Find { get; set; } = new Regex("$^");
		[LocalizationKey("Command_Argument_ReplaceRegexp_Replace")]
		public string Replace { get; set; } = string.Empty;
		[LocalizationKey("Command_Argument_ReplaceRegexp_IncludeExtension")]
		public bool IncludeExtension { get; set; } = false;

		public bool DoCommand(FileInfo file)
		{
			var filename =
				!IncludeExtension
					? Path.GetFileNameWithoutExtension(file.ChangedFilename)
					: file.ChangedFilename;
			var ext =
				!IncludeExtension
					? Path.GetExtension(file.ChangedFilename)
					: "";
			file.ChangedFilename = $"{Find.Replace(filename, Replace)}{ext}";
			return true;
		}
	}
}
