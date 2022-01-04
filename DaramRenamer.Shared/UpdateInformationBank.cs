using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Daramee.Blockar;

namespace DaramRenamer
{
	public struct UpdateInformation
	{
		public string StableLatestVersion;
		public string StableLatestUrl;

		public string UnstableLatestVersion;
		public string UnstableLatestUrl;
	}

	public enum TargetPlatform
	{
		Windows,
	}

	public static class UpdateInformationBank
	{
		private static readonly HttpClient SharedHttpClient = new HttpClient();
		
		public static async Task<UpdateInformation?> GetUpdateInformationAsync(TargetPlatform targetPlatform)
		{
			try
			{
				await using var stream = await SharedHttpClient.GetStreamAsync(
					"https://raw.githubusercontent.com/daramkun/UpdateBank/master/DaramRenamer.ini");
				return BlockarObject.DeserializeFromIni(stream, targetPlatform.ToString())
					.ToObject<UpdateInformation>();
			}
			catch
			{
				return null;
			}
		}

		public static async Task<string> DownloadFile(UpdateInformation updateInfo)
		{
			var url = new Uri(updateInfo.StableLatestUrl);
			var filename = $"DaramRenamer-{updateInfo.StableLatestVersion}.zip";
			await using var stream = await SharedHttpClient.GetStreamAsync(url);
			await using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
			await stream.CopyToAsync(fileStream);
			return filename;
		}
	}
}
