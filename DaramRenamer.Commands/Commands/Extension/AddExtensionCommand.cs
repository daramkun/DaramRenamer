using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer.Commands.Extension
{
	[Serializable, LocalizationKey ("Command_Name_AddExtension")]
	public class AddExtensionCommand : ICommand, IOrderBy
	{
		public int Order => int.MinValue;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Extension;

		[LocalizationKey ("Command_Argument_AddExtension_Extension")]
		public string Extension { get; set; } = string.Empty;

		public bool DoCommand (FileInfo file)
		{
			if (string.IsNullOrEmpty(Extension))
				return false;
			file.ChangedFilename = $"{file.ChangedFilename}{(Extension[0] != '.' ? "." : "")}{Extension}";
			return true;
		}
	}
}
