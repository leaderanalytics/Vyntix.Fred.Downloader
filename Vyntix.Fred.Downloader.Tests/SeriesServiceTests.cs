﻿namespace LeaderAnalytics.Vyntix.Fred.Downloader.Tests;

public class SeriesServiceTests: BaseTest
{
    public SeriesServiceTests(string currentProviderName) : base(currentProviderName)
    { 
    }

    [Test]
    public async Task DownloadSeriesTest()
    {
        string symbol = "GNP";
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeries(symbol, null));
        Assert.That(result.Success, Is.True);
        Assert.That(db.Series.Count(x => x.Symbol == symbol), Is.EqualTo(1));
        Assert.That(db.Series.Where(x => x.Symbol == symbol).All(x => x.LastMetadataCheck > DateTime.MinValue));
    }

    [Test]
    public async Task DownLoadSeriesAssertHasVintgatesIsTrue()
    {
        string symbol = "GNPCA"; // Has vintages
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeries(symbol, null));
        Assert.That(result.Success, Is.True);
        FredSeries? s = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        Assert.That(s, Is.Not.Null);
        Assert.That(s.HasVintages.HasValue, Is.False);
        // Get observations will set HasVintages
        RowOpResult obsResult = await client.CallAsync(x => x.ObservationsService.DownloadObservations(symbol, null));
        Assert.That(obsResult.Success, Is.True);
        s = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        Assert.That(s.HasVintages.HasValue, Is.True);
        Assert.That(s.HasVintages.Value, Is.True);
    }

    [Test]
    public async Task DownLoadSeriesAssertHasVintgatesIsFalse()
    {
        string symbol = "SP500"; // Does not have vintages
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeries(symbol, null));
        Assert.That(result.Success, Is.True);
        FredSeries? s = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        Assert.That(s, Is.Not.Null);
        Assert.That(s.HasVintages.HasValue, Is.False);
        // Get observations will set HasVintages
        RowOpResult obsResult = await client.CallAsync(x => x.ObservationsService.DownloadObservations(symbol, null));
        Assert.That(obsResult.Success, Is.True);
        s = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        Assert.That(s.HasVintages.HasValue, Is.True);
        Assert.That(s.HasVintages.Value, Is.False);
    }

   

    [Test]
    public async Task DownloadSeriesReleaseTest()
    {
        string symbol = "IRA";
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeriesRelease(symbol, null));
        Assert.That(result.Success, Is.True);
        FredSeries s = db.Series.First(x => x.Symbol == symbol);
        Assert.That(s, Is.Not.Null);
        Assert.That(s.ReleaseID, Is.Not.Null);
    }

    [Test]
    public async Task DownloadSeriesTagsTest()
    {
        string symbol = "STLFSI";
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DownloadSeriesTags(symbol, null));
        Assert.That(result.Success, Is.True);
        Assert.That(db.SeriesTags.Count(x => x.Symbol == symbol), Is.GreaterThan(0));
    }

    [Test]
    public async Task DeleteSeriesTest()
    {
        string symbol = "AAA";
        
        // Download data
        RowOpResult seriesResult = await client.CallAsync(x => x.SeriesService.DownloadSeries(symbol, null));
        RowOpResult obsResult = await client.CallAsync(x => x.ObservationsService.DownloadObservations(symbol, null));
        RowOpResult tagsResult = await client.CallAsync(x => x.SeriesService.DownloadSeriesTags(symbol, null));
        RowOpResult catResult = await client.CallAsync(x => x.CategoriesService.DownloadCategoriesForSeries(symbol, null));

        // Make sure data was saved
        Assert.That(0, Is.Not.EqualTo(db.Series.Count(x => x.Symbol == symbol)));
        Assert.That(0, Is.Not.EqualTo(db.Observations.Count(x => x.Symbol == symbol)));
        Assert.That(0, Is.Not.EqualTo(db.SeriesTags.Count(x => x.Symbol == symbol)));
        Assert.That(0, Is.Not.EqualTo(db.SeriesCategories.Count(x => x.Symbol == symbol)));

        // Delete the series
        RowOpResult result = await client.CallAsync(x => x.SeriesService.DeleteSeries(symbol));

        // Make sure data was deleted
        Assert.That(0, Is.EqualTo(db.Series.Count(x => x.Symbol == symbol)));
        Assert.That(0, Is.EqualTo(db.Observations.Count(x => x.Symbol == symbol)));
        Assert.That(0, Is.EqualTo(db.SeriesTags.Count(x => x.Symbol == symbol)));
        Assert.That(0, Is.EqualTo(db.SeriesCategories.Count(x => x.Symbol == symbol)));


    }
}
