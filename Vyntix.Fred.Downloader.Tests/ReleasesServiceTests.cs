namespace LeaderAnalytics.Vyntix.Fred.Downloader.Tests;

public class ReleasesServiceTests : BaseTest
{
    public ReleasesServiceTests(string currentProviderName) : base(currentProviderName)
    { 
    }

    [Test]
    public async Task DownloadAllReleasesTest()
    {
        RowOpResult result = await client.CallAsync(x => x.ReleasesService.DownloadAllReleases(null));
        Assert.IsTrue(result.Success);
        Assert.IsTrue(db.Releases.Any());
    }

    [Test]
    public async Task DownloadAllReleaseDatesTest()
    {
        RowOpResult result = await client.CallAsync(x => x.ReleasesService.DownloadAllReleaseDates(null));
        Assert.IsTrue(result.Success);
        Assert.IsTrue(db.ReleaseDates.Any());
    }

    [Test]
    public async Task DownloadReleaseTest()
    {
        string id = "53";
        RowOpResult result = await client.CallAsync(x => x.ReleasesService.DownloadRelease(id, null));
        Assert.IsTrue(result.Success);
        Assert.IsTrue(db.Releases.Any(x => x.NativeID == id));
    }

    [Test]
    public async Task DownloadReleaseDatesTest()
    {
        string id = "82";
        RowOpResult result = await client.CallAsync(x => x.ReleasesService.DownloadReleaseDates(id, null));
        Assert.IsTrue(result.Success);
        Assert.IsTrue(db.ReleaseDates.Any(x => x.ReleaseID == id));
    }

    [Test]
    public async Task DownloadReleaseSeriesTest()
    {
        string id = "51";
        RowOpResult result = await client.CallAsync(x => x.ReleasesService.DownloadReleaseSeries(id, null));
        Assert.IsTrue(result.Success);
        Assert.IsTrue(db.Series.Any(x => x.ReleaseID == id));
    }

    [Test]
    public async Task DownloadReleaseSourcesTest()
    {
        string id = "51";
        RowOpResult result = await client.CallAsync(x => x.ReleasesService.DownloadReleaseSources(id, null));
        Assert.IsTrue(result.Success);
        Assert.IsTrue(db.SourceReleases.Any(x => x.ReleaseNativeID == id));
    }
}
