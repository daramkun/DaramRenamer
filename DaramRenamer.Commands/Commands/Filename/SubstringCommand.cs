using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaramRenamer.Commands.Filename
{
	[Serializable, LocalizationKey ("Command_Name_Substring")]
	public class SubstringCommand : ICommand, IOrderBy
	{
		public int Order => int.MinValue + 7;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Filename;

		[LocalizationKey ("Command_Argument_Substring_StartIndex")]
		public int StartIndex { get; set; } = 0;
		[LocalizationKey ("Command_Argument_Substring_Length")]
		public int? Length { get; set; } = null;
		[LocalizationKey ("Command_Argument_Substring_IncludeExtension")]
		public bool IncludeExtension { get; set; } = false;

		public bool DoCommand(FileInfo file)
		{
			file.ChangedFilename =
				Length == null
					? IncludeExtension
						? file.ChangedFilename.Substring(StartIndex)
						: $"{Path.GetFileNameWithoutExtension(file.ChangedFilename).Substring(StartIndex)}{Path.GetExtension(file.ChangedFilename)}"
					: IncludeExtension
						? file.ChangedFilename.Substring(StartIndex, Length.Value)
						: $"{Path.GetFileNameWithoutExtension(file.ChangedFilename).Substring(StartIndex, Length.Value)}{Path.GetExtension(file.ChangedFilename)}";
			return true;
		}
	}
}
