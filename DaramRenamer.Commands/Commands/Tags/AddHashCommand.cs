using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaramRenamer.Commands.Tags
{
	[Serializable, LocalizationKey ("Command_Name_AddHash")]
	public class AddHashCommand : ICommand
	{
		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Tag;

		[LocalizationKey ("Command_Argument_AddHash_HashType")]
		public HashType HashType { get; set; }
		[LocalizationKey ("Command_Argument_AddHash_Position")]
		public Position1 Position { get; set; } = Position1.EndPoint;

		public bool DoCommand(FileInfo file)
		{
			byte [] returnValue = null;
			switch (HashType)
			{
				case HashType.MD5:
					using (Stream stream = File.Open (file.ChangedFullPath, FileMode.Open))
						returnValue = System.Security.Cryptography.MD5.Create ().ComputeHash (stream);
					break;
				case HashType.SHA1:
					using (Stream stream = File.Open (file.ChangedFullPath, FileMode.Open))
						returnValue = System.Security.Cryptography.SHA1.Create ().ComputeHash (stream);
					break;
				case HashType.SHA256:
					using (Stream stream = File.Open (file.ChangedFullPath, FileMode.Open))
						returnValue = System.Security.Cryptography.SHA256.Create ().ComputeHash (stream);
					break;
				case HashType.SHA384:
					using (Stream stream = File.Open (file.ChangedFullPath, FileMode.Open))
						returnValue = System.Security.Cryptography.SHA384.Create ().ComputeHash (stream);
					break;
				case HashType.SHA512:
					using (Stream stream = File.Open (file.ChangedFullPath, FileMode.Open))
						returnValue = System.Security.Cryptography.SHA512.Create ().ComputeHash (stream);
					break;
			}

			var sBuilder = new StringBuilder ();
			if (returnValue != null)
				foreach (var t in returnValue)
					sBuilder.Append(t.ToString("x2"));

			var hash = sBuilder.ToString ();

			var fn = Path.GetFileNameWithoutExtension (file.ChangedFilename);
			var ext = Path.GetExtension (file.ChangedFilename);
			file.ChangedFilename = Position switch
			{
				Position1.StartPoint => $"{hash}{fn}{ext}",
				Position1.EndPoint => $"{fn}{hash}{ext}",
				_ => $"{hash}{fn}{hash}{ext}"
			};

			return true;
		}
	}
}
