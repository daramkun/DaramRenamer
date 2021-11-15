using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer.Commands.FilePath
{
	[Serializable, LocalizationKey("Command_Name_RelativeGoTo")]
	public class RelativeGoToCommand : ICommand
	{
		private const string CurrentDirectory = ".";
		private const string PreviousDirectory = "..";

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Path;

		[LocalizationKey("Command_Argument_RelativeGoTo_Path")]
		public string Path { get; set; } = "";

		public bool DoCommand(FileInfo file)
		{
			var temp = new List<string>(System.IO.Path.Combine(file.ChangedPath, Path).Split('/', '\\'));
			temp.RemoveAll(dir => dir == CurrentDirectory || string.IsNullOrEmpty(dir));

			var indexOf = -1;
			while ((indexOf = temp.IndexOf(PreviousDirectory)) >= 0)
			{
				if (indexOf == 0 || indexOf == 1)
					return false;
				temp.RemoveAt(indexOf);
				temp.RemoveAt(indexOf - 1);
			}

			file.ChangedPath = string.Join(System.IO.Path.DirectorySeparatorChar, temp);
			return true;
		}
	}
}
