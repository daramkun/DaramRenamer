using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;

namespace DaramRenamer
{
	internal class Program
	{
		private static int Main(string[] args)
		{

			var version = args[0];
			var url = args[1];

			if (args.Length > 2)
			{
				var parentProcess = args[2];
				Process.GetProcessById(int.Parse(parentProcess)).WaitForExit();
			}

			var filename = $"{version}-update-file";

			var webClient = new WebClient();

			Console.WriteLine("Update System initializing...");

			Thread.Sleep(1000);

			try
			{
				Console.WriteLine("Update file downloading...");
				webClient.DownloadFile(new Uri(url), filename);
			}
			catch
			{
				Console.WriteLine("[ERROR] Update file download is failed.");
				return -1;
			}

			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, true);
				foreach (var entry in archive.Entries)
				{
					Console.WriteLine("Uncompressing {0}", entry.FullName);
					using var entryStream = entry.Open();
					using var destinationFileStream =
						new FileStream(entry.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
					entryStream.CopyTo(destinationFileStream);
				}
			}

			File.Delete(filename);

			var psInfo = new ProcessStartInfo("cmd")
			{
				Arguments = "/C start DaramRenamer.exe",
				UseShellExecute = true,
			};
			Process.Start(psInfo);

			return 0;
		}
	}
}