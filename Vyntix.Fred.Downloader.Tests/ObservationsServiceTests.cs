﻿using Microsoft.EntityFrameworkCore;

namespace LeaderAnalytics.Vyntix.Fred.Downloader.Tests;

public class ObservationsServiceTests : BaseTest
{
    public ObservationsServiceTests(string currentProviderName) : base(currentProviderName)
    { 
    }

    [Test]
    public async Task GetLocalObservationsTest()
    {
        string[] symbols = new string[] { "BAMLEM4RBLLCRPIUSEY", "CUURX000SACL1E", "CUURX100SA0" };
        RowOpResult<List<FredObservation>> data = await client.CallAsync(x => x.ObservationsService.GetLocalObservations(symbols));
        
        foreach(string symbol in symbols)
        {
            int dbCount = db.Observations.Count(x => x.Symbol == symbol);
            int memCount = data.Item.Count(x => x.Symbol == symbol);
            Assert.AreEqual(dbCount, memCount);
        }
    }
    
    [Test]
    public async Task SeriesStatisticsTest()
    {
        string symbol = "GNPCA";
        await client.CallAsync(x => x.ObservationsService.DownloadObservations(symbol));
        RowOpResult<SeriesStatistics> stats = await client.CallAsync(x => x.ObservationsService.GetSeriesStatistics(symbol));
        Assert.IsNotNull(stats);
        Assert.IsNotNull(stats.Item);
        Assert.IsTrue(stats.Success);
    }

    [Test]
    public async Task LastUpdateTest()
    {
        string symbol = "CORESTICKM159SFRBATL";
        await client.CallAsync(x => x.ObservationsService.DownloadObservations(symbol));
        DateTime lastObsCheck = db.Series.First(x => x.Symbol == symbol).LastObsCheck;
        Assert.That(lastObsCheck > DateTime.Now.AddMinutes(-5));
    }


    //[Test]
    public async Task CrashEFWithChangeTrackingError()
    {
        // https://learn.microsoft.com/en-us/ef/core/change-tracking/explicit-tracking#introduction:
        // "Attaching entities to the same DbContext instance that they were queried from should not normally be needed."
        // Really???  Was a research study done that supports this conclusion?

        // -----------

        // This test reproduces an error where an entity is attached, retrived via a query, and attached again.
        // It isolates and condenses calls made to/by several different methods starting with
        // ObservationService.DownloadObservations. 
        // This bug was fixed by adding this line to SeriesService.DownloadSeries:
        // db.Entry(series).State = EntityState.Detached; // Because in DownloadSeriesIfItDoesNotExist we get this entity again


        FredSeries s = new() { Symbol = "xyz", Title = "abc", Popularity = 1, RTStart = DateTime.Now };     // Create a new series e.g. DownloadSeries
        db.Entry(s).State = Microsoft.EntityFrameworkCore.EntityState.Added;                                // SaveSeries - Starts tracking first instance
        await db.SaveChangesAsync();                                                                        // SaveSeries

        FredSeries s2 = await db.Series.FirstOrDefaultAsync(x => x.Symbol == "xyz");                       // DownloadSeriesIfItDoesNotExist reads back a new instance

        s2.HasVintages = true;                                                                             // DownloadObservations modifies HasVintages of second instance
        db.Entry(s2).State = EntityState.Modified;                                                         // Crash 
        await db.SaveChangesAsync();
    }

    
}
