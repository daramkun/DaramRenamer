using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaramRenamer.Commands.Date
{
	[Serializable, LocalizationKey ("Command_Name_AddDate")]
	public class AddDateCommand : ICommand, IOrderBy
	{
		public int Order => int.MinValue;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Date;

		[LocalizationKey("Command_Argument_AddDate_Kind")]
		public FileDateKind Kind { get; set; } = FileDateKind.Creation;
		[LocalizationKey ("Command_Argument_AddDate_Format")]
		public string Format { get; set; } = "yyMMdd";
		[LocalizationKey ("Command_Argument_AddDate_Position")]
		public Position1 Position { get; set; } = Position1.EndPoint;

		public bool DoCommand(FileInfo file)
		{
			string fn = Path.GetFileNameWithoutExtension(file.ChangedFilename);
			string ext = Path.GetExtension(file.ChangedFilename);
			string date;
			switch (Kind)
			{
				case FileDateKind.Creation:
					date = File.GetCreationTime(file.OriginalFullPath).ToString(Format);
					break;
				case FileDateKind.LastModify:
					date = File.GetLastWriteTime(file.OriginalFullPath).ToString(Format);
					break;
				case FileDateKind.LastAccess:
					date = File.GetLastAccessTime(file.OriginalFullPath).ToString(Format);
					break;
				case FileDateKind.Now:
					date = DateTime.Now.ToString(Format);
					break;
				default: return false;
			}

			file.ChangedFilename = Position == Position1.StartPoint
				? $"{date}{fn}{ext}"
				: (Position == Position1.EndPoint
					? $"{fn}{date}{ext}"
					: $"{date}{fn}{date}{ext}");
			return true;
		}
	}
}
