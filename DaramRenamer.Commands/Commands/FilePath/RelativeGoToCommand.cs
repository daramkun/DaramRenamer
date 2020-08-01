using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer.Commands.FilePath
{
	[Serializable, LocalizationKey("Command_Name_RelativeGoTo")]
	public class RelativeGoToCommand : ICommand
	{
		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Path;

		[LocalizationKey ("Command_Argument_RelativeGoTo_Path")]
		public string Path { get; set; } = "";

		public bool DoCommand (FileInfo file)
		{
			file.ChangedPath = System.IO.Path.Combine(file.ChangedPath, Path);
			return true;
		}
	}
}
