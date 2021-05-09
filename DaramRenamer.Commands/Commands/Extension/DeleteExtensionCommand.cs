using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaramRenamer.Commands.Extension
{
	[Serializable, LocalizationKey ("Command_Name_DeleteExtension")]
	public class DeleteExtensionCommand : ICommand
	{
		public int Order => int.MinValue + 2;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Extension;

		[LocalizationKey("Commamd_Argument_DeleteExtension_ApplyToDirectory")]
		public bool ApplyToDirectory { get; set; } = false;

		public bool DoCommand(FileInfo file)
		{
			if (!ApplyToDirectory && file.IsDirectory)
				return true;

			file.ChangedFilename = Path.GetFileNameWithoutExtension(file.ChangedFilename);
			return true;
		}
	}
}
