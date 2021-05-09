using System.Net;
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
		public static UpdateInformation? GetUpdateInformation(TargetPlatform targetPlatform)
		{
			try
			{
				var webRequest =
					WebRequest.CreateHttp("https://raw.githubusercontent.com/daramkun/UpdateBank/master/DaramRenamer.ini");
				var webResponse = webRequest.GetResponse();
				using var stream = webResponse.GetResponseStream();
				return BlockarObject.DeserializeFromIni(stream, targetPlatform.ToString()).ToObject<UpdateInformation>();
			}
			catch
			{
				return null;
			}
		}
	}
}
