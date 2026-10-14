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
    public void RetainsVersionWhenReleaseHasNoAssetForRuntimeIdentifier()
    {
        const string json = """
                            {
                              "tag_name": "2024.7.3",
                              "assets": [
                                {
                                  "name": "DaramRenamer_2024.7.3.zip",
                                  "browser_download_url": "https://github.com/daramkun/DaramRenamer/releases/download/2024.7.3/DaramRenamer_2024.7.3.zip"
                                }
                              ]
                            }
                            """;

        var update = UpdateInformationBank.ParseReleaseJson(json, "win-x64");

        Assert.IsNotNull(update);
        Assert.AreEqual("2024.7.3", update.Value.StableLatestVersion);
        Assert.IsNull(update.Value.StableLatestUrl);
        Assert.IsNull(UpdateInformationBank.ParseReleaseJson(json, "osx-x64"));
    }
}
