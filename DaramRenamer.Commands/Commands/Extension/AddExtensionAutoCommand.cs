using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Daramee.FileTypeDetector;

namespace DaramRenamer.Commands.Extension
{
	[Serializable, LocalizationKey("Command_Name_AddExtensionAuto")]
	public sealed class AddExtensionAutoCommand : ICommand, IOrderBy
	{
		public int Order => int.MinValue + 1;

		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Extension;

		public AddExtensionAutoCommand()
		{
			DetectorService.AddDetectors(Assembly.Load(new AssemblyName("DaramRenamer.Commands")));
		}

		public bool DoCommand(FileInfo file)
		{
			if (!File.Exists(file.OriginalFullPath))
				return false;

			using Stream stream = File.OpenRead(file.OriginalFullPath);
			var detector = DetectorService.DetectDetector(stream);

			if (detector == null)
				return false;

			file.ChangedFilename = $"{file.ChangedFilename}.{detector.Extension}";

			return true;
		}
	}
}
