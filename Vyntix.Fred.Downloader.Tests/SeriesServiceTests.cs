namespace LeaderAnalytics.Vyntix.Fred.Downloader.Tests;

public class SeriesServiceTests: BaseTest
{
    public SeriesServiceTests(string currentProviderName) : base(currentProviderName)
    { 
    }

    [Test]
    public async Task DownloadSeriesTest()
    {
        string symbol = "GNP";
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeries(symbol));
        Assert.IsTrue(result.Success);
        Assert.That(db.Series.Count(x => x.Symbol == symbol), Is.EqualTo(1));
        Assert.That(db.Series.Where(x => x.Symbol == symbol).All(x => x.LastMetadataCheck > DateTime.MinValue));
    }

    [Test]
    public async Task DownLoadSeriesAssertHasVintgatesIsTrue()
    {
        string symbol = "GNPCA"; // Has vintages
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeries(symbol));
        Assert.IsTrue(result.Success);
        FredSeries? s = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        Assert.That(s, Is.Not.Null);
        Assert.That(s.HasVintages.HasValue, Is.False);
        // Get observations will set HasVintages
        RowOpResult obsResult = await client.CallAsync(x => x.ObservationsService.DownloadObservations(symbol));
        Assert.IsTrue(obsResult.Success);
        s = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        Assert.That(s.HasVintages.HasValue, Is.True);
        Assert.That(s.HasVintages.Value, Is.True);
    }

    [Test]
    public async Task DownLoadSeriesAssertHasVintgatesIsFalse()
    {
        string symbol = "SP500"; // Does not have vintages
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeries(symbol));
        Assert.IsTrue(result.Success);
        FredSeries? s = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        Assert.That(s, Is.Not.Null);
        Assert.That(s.HasVintages.HasValue, Is.False);
        // Get observations will set HasVintages
        RowOpResult obsResult = await client.CallAsync(x => x.ObservationsService.DownloadObservations(symbol));
        Assert.IsTrue(obsResult.Success);
        s = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        Assert.That(s.HasVintages.HasValue, Is.True);
        Assert.That(s.HasVintages.Value, Is.False);
    }

   

    [Test]
    public async Task DownloadSeriesReleaseTest()
    {
        string symbol = "IRA";
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeriesRelease(symbol));
        Assert.IsTrue(result.Success);
        FredSeries s = db.Series.First(x => x.Symbol == symbol);
        Assert.That(s, Is.Not.Null);
        Assert.That(s.ReleaseID, Is.Not.Null);
    }

    [Test]
    public async Task DownloadSeriesTagsTest()
    {
        string symbol = "STLFSI";
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeriesTags(symbol));
        Assert.IsTrue(result.Success);
        Assert.That(db.SeriesTags.Count(x => x.Symbol == symbol), Is.GreaterThan(0));
    }

    [Test]
    public async Task DeleteSeriesTest()
    {
        string symbol = "AAA";
        
        // Download data
        RowOpResult seriesResult = await client.CallAsync(x => x.SeriesService.DownloadSeries(symbol));
        RowOpResult obsResult = await client.CallAsync(x => x.ObservationsService.DownloadObservations(symbol));
        RowOpResult tagsResult = await client.CallAsync(x => x.SeriesService.DownloadSeriesTags(symbol));
        RowOpResult catResult = await client.CallAsync(x => x.CategoriesService.DownloadCategoriesForSeries(symbol));

        // Make sure data was saved
        Assert.AreNotEqual(0, db.Series.Count(x => x.Symbol == symbol));
        Assert.AreNotEqual(0, db.Observations.Count(x => x.Symbol == symbol));
        Assert.AreNotEqual(0, db.SeriesTags.Count(x => x.Symbol == symbol));
        Assert.AreNotEqual(0, db.SeriesCategories.Count(x => x.Symbol == symbol));

        // Delete the series
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DeleteSeries(symbol));

        // Make sure data was deleted
        Assert.AreEqual(0, db.Series.Count(x => x.Symbol == symbol));
        Assert.AreEqual(0, db.Observations.Count(x => x.Symbol == symbol));
        Assert.AreEqual(0, db.SeriesTags.Count(x => x.Symbol == symbol));
        Assert.AreEqual(0, db.SeriesCategories.Count(x => x.Symbol == symbol));


    }
}
