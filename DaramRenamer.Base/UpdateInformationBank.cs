using System.Net.Http.Headers;
using System.Text.Json;

namespace DaramRenamer;

public readonly record struct UpdateInformation(string StableLatestVersion, string? StableLatestUrl);

public static class UpdateInformationBank
{
    private const string LatestReleaseApiUrl = "https://api.github.com/repos/daramkun/DaramRenamer/releases/latest";
    private static readonly HttpClient SharedHttpClient = CreateHttpClient();

    public static async Task<UpdateInformation?> GetUpdateInformationAsync(string runtimeIdentifier)
    {
        if (GetArchiveExtension(runtimeIdentifier) is null)
            return null;

        try
        {
            using var response = await SharedHttpClient.GetAsync(
                LatestReleaseApiUrl,
                HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(responseStream);
            return ParseRelease(document.RootElement, runtimeIdentifier);
        }
        catch
        {
            return null;
        }
    }

    internal static UpdateInformation? ParseReleaseJson(string json, string runtimeIdentifier)
    {
        if (GetArchiveExtension(runtimeIdentifier) is null)
            return null;

        using var document = JsonDocument.Parse(json);
        return ParseRelease(document.RootElement, runtimeIdentifier);
    }

    private static UpdateInformation? ParseRelease(JsonElement release, string runtimeIdentifier)
    {
        var archiveExtension = GetArchiveExtension(runtimeIdentifier);
        if (archiveExtension is null ||
            !release.TryGetProperty("tag_name", out var tagElement) ||
            tagElement.ValueKind != JsonValueKind.String)
            return null;

        var version = tagElement.GetString();
        if (string.IsNullOrWhiteSpace(version) ||
            !release.TryGetProperty("assets", out var assetsElement) ||
            assetsElement.ValueKind != JsonValueKind.Array)
            return null;

        var expectedAssetName = $"DaramRenamer-{version}-{runtimeIdentifier}.{archiveExtension}";
        foreach (var asset in assetsElement.EnumerateArray())
        {
            if (!asset.TryGetProperty("name", out var nameElement) ||
                !string.Equals(nameElement.GetString(), expectedAssetName, StringComparison.Ordinal) ||
                !asset.TryGetProperty("browser_download_url", out var urlElement))
                continue;

            var downloadUrl = urlElement.GetString();
            if (Uri.TryCreate(downloadUrl, UriKind.Absolute, out var uri) &&
                uri.Scheme == Uri.UriSchemeHttps)
                return new UpdateInformation(version, uri.AbsoluteUri);
        }

        return new UpdateInformation(version, null);
    }

    private static string? GetArchiveExtension(string runtimeIdentifier) =>
        runtimeIdentifier switch
        {
            "win-x64" or "win-arm64" => "zip",
            "linux-x64" or "osx-arm64" => "tar.gz",
            _ => null
        };

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DaramRenamer", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }
}
