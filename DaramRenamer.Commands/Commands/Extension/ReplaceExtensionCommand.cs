using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaramRenamer.Commands.Extension
{
	[Serializable, LocalizationKey ("Command_Name_ReplaceExtension")]
	public class ReplaceExtensionCommand : ICommand
	{
		public int Order => int.MinValue + 3;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Extension;

		[LocalizationKey ("Command_Argument_ReplaceExtension_Extension")]
		public string Extension { get; set; } = string.Empty;

		public bool DoCommand(FileInfo file)
		{
			if (string.IsNullOrEmpty(Extension)) return false;
			file.ChangedFilename = $"{Path.GetFileNameWithoutExtension(file.ChangedFilename)}{(Extension [0] != '.' ? "." : "")}{Extension}";
			return true;
		}
	}
}
