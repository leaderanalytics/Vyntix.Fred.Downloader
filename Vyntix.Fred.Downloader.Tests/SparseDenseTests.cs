﻿using LeaderAnalytics.Vyntix.Fred.Domain;

namespace LeaderAnalytics.Vyntix.Fred.Downloader.Tests;

public class SparseDenseTests : BaseTest
{
    private VintageComposerService vintageComposerService;

    public SparseDenseTests(string currentProviderName) : base(currentProviderName)
    {
    }

    [SetUp]
    protected override async Task Setup()
    {
        await base.Setup();
        vintageComposerService = scope.Resolve<VintageComposerService>();
    }

    [Test]
    public async Task SaveChunksTest()
    {
        // https://alfred.stlouisfed.org/help/downloaddata#outputformats

        string symbol = "gdp";
        DateTime vintage1 = new(2022,  9, 29);
        DateTime vintage2 = new(2022, 10, 27);
        DateTime vintage3 = new(2022, 11, 30);
        DateTime vintage4 = new(2022, 12, 22);

        List<FredObservation> observations = (await fredClient.GetObservations(symbol, new List<DateTime> { vintage1, vintage2 }, null, null, DataDensity.Sparse)).Data;
        List<FredObservation> observations2 = (await fredClient.GetObservations(symbol, new List<DateTime> { vintage3, vintage4 }, null, null, DataDensity.Sparse)).Data;

        // Get all four vintages in two seperate chunks
        observations.ForEach(x => x.Symbol = "gdp_2_chunks");
        observations2.ForEach(x => x.Symbol = "gdp_2_chunks");
        await db.Observations.AddRangeAsync(observations);
        await db.Observations.AddRangeAsync(observations2);
        await db.SaveChangesAsync();


        // Get all four vintages in one chunk.
        observations = (await fredClient.GetObservations(symbol, new List<DateTime> { vintage1, vintage2, vintage3, vintage4 }, null, null, DataDensity.Sparse)).Data;
        observations.ForEach(x => x.Symbol = "gdp_1_chunk");
        await db.Observations.AddRangeAsync(observations);
        await db.SaveChangesAsync();
    }
}
