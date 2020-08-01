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

		public bool DoCommand(FileInfo file)
		{
			file.ChangedFilename = Path.GetFileNameWithoutExtension(file.ChangedFilename);
			return true;
		}
	}
}
