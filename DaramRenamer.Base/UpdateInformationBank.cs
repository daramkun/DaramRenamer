using System.Net.Http;

namespace DaramRenamer;

public struct UpdateInformation
{
    public string StableLatestVersion;
    public string StableLatestUrl;

    public string UnstableLatestVersion;
    public string UnstableLatestUrl;
}

public enum TargetPlatform
{
    Windows
}

public static class UpdateInformationBank
{
    private static readonly HttpClient SharedHttpClient = new();

    public static async Task<UpdateInformation?> GetUpdateInformationAsync(TargetPlatform targetPlatform)
    {
        try
        {
            var ini = await SharedHttpClient.GetStringAsync(
                "https://raw.githubusercontent.com/daramkun/UpdateBank/master/DaramRenamer.ini");
            return Parse(ini, targetPlatform.ToString());
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

    private static UpdateInformation? Parse(string ini, string sectionName)
    {
        var section = string.Empty;
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StringReader(ini);
        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                section = line[1..^1].Trim();
                continue;
            }

            if (!section.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                continue;

            var equals = line.IndexOf('=');
            if (equals <= 0)
                continue;

            values[line[..equals].Trim()] = line[(equals + 1)..].Trim();
        }

        if (values.Count == 0)
            return null;

        return new UpdateInformation
        {
            StableLatestVersion = Get(values, nameof(UpdateInformation.StableLatestVersion)),
            StableLatestUrl = Get(values, nameof(UpdateInformation.StableLatestUrl)),
            UnstableLatestVersion = Get(values, nameof(UpdateInformation.UnstableLatestVersion)),
            UnstableLatestUrl = Get(values, nameof(UpdateInformation.UnstableLatestUrl))
        };
    }

    private static string Get(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) ? value : string.Empty;
}
