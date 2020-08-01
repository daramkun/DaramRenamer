using System;
using System.IO;

namespace DaramRenamer.Commands.Filename
{
	[Serializable, LocalizationKey ("Command_Name_Trim")]
	public class TrimCommand : ICommand, IOrderBy
	{
		public int Order => int.MinValue + 4;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Filename;

		[LocalizationKey ("Command_Argument_Trim_Position")]
		public Position2 Position { get; set; } = Position2.BothPoint;

		public bool DoCommand(FileInfo file)
		{
			var filename = Path.GetFileNameWithoutExtension(file.ChangedFilename);
			filename = Position == Position2.StartPoint
				? filename.TrimStart()
				: Position == Position2.EndPoint
					? filename.TrimEnd()
					: filename.Trim();
			file.ChangedFilename = $"{filename}{Path.GetExtension(file.ChangedFilename)}";
			return true;
		}
	}
}
