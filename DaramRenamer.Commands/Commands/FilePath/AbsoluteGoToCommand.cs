using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaramRenamer.Commands.FilePath
{
	[Serializable, LocalizationKey("Command_Name_AbsoluteGoTo")]
	public class AbsoluteGoToCommand : ICommand
	{
		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Path;

		[LocalizationKey("Command_Argument_AbsoluteGoTo_Path")]
		public DirectoryInfo Path { get; set; } = new DirectoryInfo(Environment.CurrentDirectory);

		public bool DoCommand(FileInfo file)
		{
			if (!Path.Exists) return false;
			file.ChangedPath = Path.FullName;
			return true;
		}
	}
}
