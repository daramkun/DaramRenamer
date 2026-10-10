namespace DaramRenamer.Test;

[TestClass]
public class UpdateInformationBankTests
{
    [TestMethod]
    [DataRow("win-x64", "DaramRenamer-2026.10.1-win-x64.zip")]
    [DataRow("win-arm64", "DaramRenamer-2026.10.1-win-arm64.zip")]
    [DataRow("linux-x64", "DaramRenamer-2026.10.1-linux-x64.tar.gz")]
    [DataRow("osx-arm64", "DaramRenamer-2026.10.1-osx-arm64.tar.gz")]
    public void SelectsReleaseAssetForRuntimeIdentifier(string runtimeIdentifier, string assetName)
    {
        var json = $$"""
                     {
                       "tag_name": "2026.10.1",
                       "assets": [
                         {
                           "name": "{{assetName}}",
                           "browser_download_url": "https://github.com/daramkun/DaramRenamer/releases/download/2026.10.1/{{assetName}}"
                         }
                       ]
                     }
                     """;

        var update = UpdateInformationBank.ParseReleaseJson(json, runtimeIdentifier);

        Assert.IsNotNull(update);
        Assert.AreEqual("2026.10.1", update.Value.StableLatestVersion);
        Assert.AreEqual(
            $"https://github.com/daramkun/DaramRenamer/releases/download/2026.10.1/{assetName}",
            update.Value.StableLatestUrl);
    }

    [TestMethod]
    public void ReturnsNullWhenReleaseHasNoAssetForRuntimeIdentifier()
    {
        const string json = """
                            {
                              "tag_name": "2026.10.1",
                              "assets": [
                                {
                                  "name": "DaramRenamer-2026.10.1-win-x64.zip",
                                  "browser_download_url": "https://github.com/daramkun/DaramRenamer/releases/download/2026.10.1/DaramRenamer-2026.10.1-win-x64.zip"
                                }
                              ]
                            }
                            """;

        Assert.IsNull(UpdateInformationBank.ParseReleaseJson(json, "osx-arm64"));
        Assert.IsNull(UpdateInformationBank.ParseReleaseJson(json, "osx-x64"));
    }
}
