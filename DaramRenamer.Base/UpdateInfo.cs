namespace DaramRenamer;

public enum UpdatePlatform
{
    Windows,
    macOS,
}

[Serializable]
public class UpdateInfo
{
    private static readonly HttpClient HttpClient = new();
    private const string Uri = "https://raw.githubusercontent.com/daramkun/UpdateBank/master/DaramRenamer.ini";

    public static async Task<UpdateInfo?> GetUpdateInfo(UpdatePlatform platform)
    {
        try
        {
            var findingTarget = $"[{platform}]";
            
            var rawData = (await HttpClient.GetStringAsync(Uri)).Split('\n');
            var found = false;
            var stableLatestVersion = string.Empty;
            var stableLatestUrl = string.Empty;
            foreach (var line in rawData)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                
                if (found)
                {
                    if (line.StartsWith('['))
                        break;
                    
                    if (line.StartsWith("StableLatestVersion=", StringComparison.OrdinalIgnoreCase))
                        stableLatestVersion = line["StableLatestVersion=".Length..];
                    else if (line.StartsWith("StableLatestUrl=", StringComparison.OrdinalIgnoreCase))
                        stableLatestUrl = line["StableLatestUrl=".Length..];
                }
                else
                {
                    if (line.Equals(findingTarget, StringComparison.OrdinalIgnoreCase))
                        found = true;
                }
            }

            return new UpdateInfo(stableLatestVersion, stableLatestUrl);
        }
        catch
        {
            return null;
        }
    }

    public string StableLatestVersion { get; }
    public string StableLatestUrl { get; }
    
    private UpdateInfo(string stableLatestVersion, string stableLatestUrl)
    {
        StableLatestVersion = stableLatestVersion;
        StableLatestUrl = stableLatestUrl;
    }
}