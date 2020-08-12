using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DaramRenamer.Commands.Tags
{
	[Serializable, LocalizationKey ("Command_Name_AddGitInfo")]
	public class AddGitInfoCommand : ICommand
	{
		public bool ParallelProcessable => true;
		public CommandCategory Category => CommandCategory.Tag;

		[LocalizationKey ("Command_Argument_AddGitInfo_GitInfo")]
		public GitInfo GitInfo { get; set; } = GitInfo.CommitId;
		[LocalizationKey ("Command_Argument_AddGitInfo_Position")]
		public Position1 Position { get; set; } = Position1.EndPoint;

		private readonly string[] Arguments = 
		{
			"rev-parse HEAD",
			"rev-parse --short HEAD",
			"branch",
			"rev-list --count --all",
		};

		public bool DoCommand(FileInfo file)
		{
			var psInfo = new ProcessStartInfo("git", Arguments[(int)GitInfo])
			{
				UseShellExecute = false,
				WorkingDirectory = file.OriginalPath,
				RedirectStandardOutput = true,
				StandardOutputEncoding = Encoding.UTF8,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
			};

			Process process;
			try
			{
				process = Process.Start (psInfo);
				process?.WaitForExit();
				if (process?.ExitCode != 0)
					return false;
			}
			catch
			{
				return false;
			}

			var value = process.StandardOutput.ReadToEnd().Trim();
			if (GitInfo == GitInfo.BranchName)
			{
				var list = value.Split('\n');
				foreach (var item in list)
				{
					if (item[0] != '*')
						continue;
					value = item.Substring(2);
					break;
				}
			}

			var fn = Path.GetFileNameWithoutExtension (file.ChangedFilename);
			var ext = Path.GetExtension (file.ChangedFilename);
			file.ChangedFilename = Position switch
			{
				Position1.StartPoint => $"{value}{fn}{ext}",
				Position1.EndPoint => $"{fn}{value}{ext}",
				_ => $"{value}{fn}{value}{ext}"
			};

			return true;
		}
	}
}
