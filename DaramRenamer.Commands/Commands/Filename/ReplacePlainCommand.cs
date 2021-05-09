using System;
using System.IO;

namespace DaramRenamer.Commands.Filename
{
	[Serializable, LocalizationKey("Command_Name_ReplacePlain")]
	public class ReplacePlainCommand : ICommand, IOrderBy
	{
		public int Order => int.MinValue;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Filename;

		[LocalizationKey("Command_Argument_ReplacePlain_Find")]
		public string Find { get; set; } = string.Empty;
		[LocalizationKey("Command_Argument_ReplacePlain_Replace")]
		public string Replace { get; set; } = string.Empty;
		[LocalizationKey("Command_Argument_ReplacePlain_IncludeExtension")]
		public bool IncludeExtension { get; set; } = false;

		public bool DoCommand(FileInfo file)
		{
			if (string.IsNullOrEmpty(Find))
				return false;
			Replace ??= "";

			file.ChangedFilename =
				IncludeExtension
					? $"{file.ChangedFilename.Replace(Find, Replace)}"
					: $"{Path.GetFileNameWithoutExtension(file.ChangedFilename).Replace(Find, Replace)}{Path.GetExtension(file.ChangedFilename)}";

			return true;
		}
	}
}
