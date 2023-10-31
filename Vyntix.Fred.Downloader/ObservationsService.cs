namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class ObservationsService : BaseService, IObservationsService
{
    private Func<string, FREDStagingDb> dbFactory;

    public ObservationsService(FREDStagingDb db, IAPI_Manifest downloaderServices, IFredClient fredClient, Func<string, FREDStagingDb> dbFactory) : base(db, downloaderServices, fredClient)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<List<RowOpResult>> DownloadObservations(string[] symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);

        List<RowOpResult> result = new List<RowOpResult>();

        foreach(string symbol in symbols)
            result.Add(await DownloadObservations(symbol));

        return result;
    }

    public async Task<RowOpResult> DownloadObservations(string symbol)
    {
        RowOpResult seriesResult = await serviceManifest.SeriesService.DownloadSeriesIfItDoesNotExist(symbol);
        
        if (!seriesResult.Success)
            return seriesResult;

        RowOpResult result = new RowOpResult();

        DateTime lastVintageDate = (await db.Observations.Where(x => x.Symbol == symbol).MaxAsync(x => (DateTime?)x.VintageDate)) ?? new DateTime(1776, 7, 3);
        List<DateTime> vintages = (await fredClient.GetVintageDates(symbol, lastVintageDate.AddDays(1), null));

        if (vintages?.Any() ?? false)
        {
            List<FredObservation>  observations = await fredClient.GetObservations(symbol, vintages, null, null, DataDensity.Sparse);

            if (observations?.Any() ?? false)
            {
                await db.Observations.AddRangeAsync(observations);
                await db.SaveChangesAsync();
            }
        }
        result.Success = true;
        return result;
    }

    public async Task<RowOpResult> DeleteObservationsForSymbol(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult result = new();
        await db.Observations.Where(x => x.Symbol == symbol).ExecuteDeleteAsync();
        result.Success = true;
        return result;
    }


    public async Task<RowOpResult<List<FredObservation>>> GetLocalObservations(string[] symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        RowOpResult<List<FredObservation>> result = new();
        string allSymbols = string.Join(',', symbols);
        result.Item = await db.Observations.Where(x => EF.Functions.Like(x.Symbol, $"%{allSymbols}%")).OrderBy(x => x.Symbol).ThenBy(x => x.ObsDate).ThenBy(x => x.VintageDate).ToListAsync();
        result.Success = true;
        return result;
    }

    public async Task<RowOpResult<SeriesStatistics>> GetSeriesStatistics(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult<SeriesStatistics> result = new RowOpResult<SeriesStatistics> { Item = new SeriesStatistics()};
        
        if (!await db.Observations.Where(x => x.Symbol == symbol).AnyAsync())
            return result;
        
        string connectionString = db.Database.GetConnectionString();
        Task<int> obsCountTask = dbFactory(connectionString).Observations.Where(x => x.Symbol == symbol).Select(x => x.ObsDate).Distinct().CountAsync();
        Task<int> vintCountTask = dbFactory(connectionString).Observations.Where(x => x.Symbol == symbol).Select(x => x.VintageDate).Distinct().CountAsync();
        Task<int> nullCountTask = dbFactory(connectionString).Observations.Where(x => x.Symbol == symbol && x.Value == null).CountAsync();
        Task<DateTime> firstVintageDateTask = dbFactory(connectionString).Observations.Where(x => x.Symbol == symbol).MinAsync(x => x.VintageDate);
        Task<DateTime> lastVintageDateTask = dbFactory(connectionString).Observations.Where(x => x.Symbol == symbol).MaxAsync(x => x.VintageDate);
        Task<DateTime> firstObsDateTask = dbFactory(connectionString).Observations.Where(x => x.Symbol == symbol).MinAsync(x => x.ObsDate);
        Task<DateTime> lastObsDateTask = dbFactory(connectionString).Observations.Where(x => x.Symbol == symbol).MaxAsync(x => x.ObsDate);
        Task.WaitAll(obsCountTask, vintCountTask, nullCountTask, firstVintageDateTask, lastVintageDateTask, firstObsDateTask, lastObsDateTask);

        result.Item.ObservationCount = obsCountTask.Result;
        result.Item.VintageCount = vintCountTask.Result;
        result.Item.NullValueCount = nullCountTask.Result;
        result.Item.FirstObservationDate = firstObsDateTask.Result;
        result.Item.LastObservationDate = lastObsDateTask.Result;
        result.Item.FirstVintageDate = firstVintageDateTask.Result;
        result.Item.LastVintageDate = lastVintageDateTask.Result;
        result.Success = true;
        return result;
    }
}
