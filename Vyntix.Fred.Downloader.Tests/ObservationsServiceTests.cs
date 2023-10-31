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


    }
}
