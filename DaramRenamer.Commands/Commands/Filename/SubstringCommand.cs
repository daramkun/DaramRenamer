using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaramRenamer.Commands.Filename
{
	[Serializable, LocalizationKey("Command_Name_Substring")]
	public class SubstringCommand : ICommand, IOrderBy
	{
		public int Order => int.MinValue + 8;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Filename;

		[LocalizationKey("Command_Argument_Substring_StartIndex")]
		public uint StartIndex { get; set; } = 0;
		[LocalizationKey("Command_Argument_Substring_Length")]
		public uint? Length { get; set; } = null;
		[LocalizationKey("Command_Argument_Substring_IncludeExtension")]
		public bool IncludeExtension { get; set; } = false;

		public bool DoCommand(FileInfo file)
		{
			if (file.ChangedFilename.Length <= StartIndex)
				return false;
			if (Length != null && file.ChangedFilename.Length <= StartIndex + Length)
				return false;
		
			file.ChangedFilename =
				Length == null
					? IncludeExtension
						? file.ChangedFilename.Substring((int)StartIndex)
						: $"{Path.GetFileNameWithoutExtension(file.ChangedFilename).Substring((int)StartIndex)}{Path.GetExtension(file.ChangedFilename)}"
					: IncludeExtension
						? file.ChangedFilename.Substring((int)StartIndex, (int)Length.Value)
						: $"{Path.GetFileNameWithoutExtension(file.ChangedFilename).Substring((int)StartIndex, (int)Length.Value)}{Path.GetExtension(file.ChangedFilename)}";
			return true;
		}
	}
}
