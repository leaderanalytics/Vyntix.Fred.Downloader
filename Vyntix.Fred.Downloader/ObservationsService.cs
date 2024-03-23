namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class ObservationsService : BaseService, IObservationsService
{
    private Func<string, FREDStagingDb> dbFactory;
    private IEndPointConfiguration currentEndpoint;
    private ResolutionHelper resolutionHelper;

    public ObservationsService(
        FREDStagingDb db, 
        IAPI_Manifest downloaderServices, 
        IFredClient fredClient, 
        IAdaptiveClient<IAPI_Manifest> serviceClient, 
        ResolutionHelper resolutionHelper, 
        ILogger<ObservationsService> logger
    ) : base(db, downloaderServices, fredClient, logger)
    {
        currentEndpoint = serviceClient.CurrentEndPoint ?? throw new NullReferenceException("CurrentEndPoint is null");
        this.resolutionHelper = resolutionHelper ?? throw new ArgumentNullException(nameof(resolutionHelper));
    }

    public async Task<List<RowOpResult>> DownloadObservations(string[] symbols)
    {
        logger.LogDebug("Starting {m}. Parameters are {@p1}", nameof(DownloadObservations), symbols);
        ArgumentNullException.ThrowIfNull(symbols);

        List<RowOpResult> result = new List<RowOpResult>();

        foreach(string symbol in symbols)
            result.Add(await DownloadObservations(symbol));

        logger.LogDebug("{m} complete.", nameof(DownloadObservations));
        return result;
    }

    public async Task<RowOpResult> DownloadObservations(string symbol)
    {
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadObservations), symbol);
        RowOpResult result = new();
        RowOpResult<FredSeries> seriesResult = await serviceManifest.SeriesService.DownloadSeriesIfItDoesNotExist(symbol);

        if (!seriesResult.Success)
        {
            result.Message = seriesResult.Message;
            return result;
        }
        
        List<FredObservation> observations = new(4000);
        DateTime lastVintageDate = (await db.Observations.Where(x => x.Symbol == symbol).MaxAsync(x => (DateTime?)x.VintageDate))?.AddDays(1) ?? new DateTime(1776, 7, 4);

        if ((!seriesResult.Item.HasVintages.HasValue) || seriesResult.Item.HasVintages.Value)
        {
            // HasVintages is null or series has vintages.  When HasVintages is null we default to attempting to download vintages. 
            APIResult<List<DateTime>> vintageResult = (await fredClient.GetVintageDates(symbol, lastVintageDate, null));

            if (vintageResult.Success)
            {
                // series has vintages
                seriesResult.Item.HasVintages = true;

                if ((vintageResult.Data?.Any() ?? false))
                    observations = (await fredClient.GetObservations(symbol, vintageResult.Data, null, null, DataDensity.Sparse)).Data;
            }
            else
            {
                // series does not have vintages
                seriesResult.Item.HasVintages = false;
            }
        }

        if (!seriesResult.Item.HasVintages.Value)
            observations = await fredClient.GetNonVintageObservations(symbol, lastVintageDate, null);

        
        bool anyObs = observations?.Any() ?? false;

        seriesResult.Item.LastObsCheck = DateTime.Now.ToUniversalTime();
        await serviceManifest.SeriesService.SaveSeries(seriesResult.Item, ! anyObs); 

        if (anyObs)
        {
            await db.Observations.AddRangeAsync(observations);
            await db.SaveChangesAsync();
        }
        
        result.Success = true;
        logger.LogDebug("{m} complete.", nameof(DownloadObservations));
        return result;
    }

    public async Task<RowOpResult> DeleteObservationsForSymbol(string symbol)
    {
        logger.LogInformation("Starting {m}. Parameters are {p1}", nameof(DeleteObservationsForSymbol), symbol);
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult result = new();
        await db.Observations.Where(x => x.Symbol == symbol).ExecuteDeleteAsync();
        result.Success = true;
        logger.LogInformation("{m} complete.", nameof(DeleteObservationsForSymbol));
        return result;
    }


    public async Task<RowOpResult<List<FredObservation>>> GetLocalObservations(string[] symbols)
    {
        logger.LogDebug("Starting {m}. Parameters are {@p1}", nameof(GetLocalObservations), symbols);
        ArgumentNullException.ThrowIfNull(symbols);
        RowOpResult<List<FredObservation>> result = new();
        string allSymbols = string.Join(',', symbols);
        result.Item = await db.Observations.Where(x => EF.Functions.Like(x.Symbol, $"%{allSymbols}%")).OrderBy(x => x.Symbol).ThenBy(x => x.ObsDate).ThenBy(x => x.VintageDate).ToListAsync();
        result.Success = true;
        logger.LogDebug("{m} complete.", nameof(GetLocalObservations));
        return result;
    }

    public async Task<RowOpResult<SeriesStatistics>> GetSeriesStatistics(string symbol)
    {
        logger.LogDebug("Starting {m}. Parameters are {@p1}", nameof(GetSeriesStatistics), symbol);
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult<SeriesStatistics> result = new RowOpResult<SeriesStatistics> { Item = new SeriesStatistics()};
        
        if (!await db.Observations.Where(x => x.Symbol == symbol).AnyAsync())
            return result;

        FREDStagingDb db1 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;
        FREDStagingDb db2 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;
        FREDStagingDb db3 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;
        FREDStagingDb db4 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;
        FREDStagingDb db5 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;
        FREDStagingDb db6 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;
        FREDStagingDb db7 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;
        FREDStagingDb db8 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;
        FREDStagingDb db9 = resolutionHelper.ResolveDbContext(currentEndpoint) as FREDStagingDb;

        Task<int> obsCountTask = db1.Observations.Where(x => x.Symbol == symbol).Select(x => x.ObsDate).Distinct().CountAsync();
        Task<int> vintCountTask = db2.Observations.Where(x => x.Symbol == symbol).Select(x => x.VintageDate).Distinct().CountAsync();
        Task<int> nullCountTask = db3.Observations.Where(x => x.Symbol == symbol && x.Value == null).CountAsync();
        Task<DateTime> firstVintageDateTask = db4.Observations.Where(x => x.Symbol == symbol).MinAsync(x => x.VintageDate);
        Task<DateTime> lastVintageDateTask = db5.Observations.Where(x => x.Symbol == symbol).MaxAsync(x => x.VintageDate);
        Task<DateTime> firstObsDateTask = db6.Observations.Where(x => x.Symbol == symbol).MinAsync(x => x.ObsDate);
        Task<DateTime> lastObsDateTask = db7.Observations.Where(x => x.Symbol == symbol).MaxAsync(x => x.ObsDate);
        Task<decimal> minValueTask = db8.Observations.Where(x => x.Symbol == symbol && x.Value.HasValue).MinAsync(x => x.Value ?? 0);
        Task<decimal> maxValueTask = db9.Observations.Where(x => x.Symbol == symbol && x.Value.HasValue).MaxAsync(x => x.Value ?? 0);
        Task.WaitAll(obsCountTask, vintCountTask, nullCountTask, firstVintageDateTask, lastVintageDateTask, firstObsDateTask, lastObsDateTask, minValueTask, maxValueTask);

        result.Item.ObservationCount = obsCountTask.Result;
        result.Item.VintageCount = vintCountTask.Result;
        result.Item.NullValueCount = nullCountTask.Result;
        result.Item.FirstObservationDate = firstObsDateTask.Result;
        result.Item.LastObservationDate = lastObsDateTask.Result;
        result.Item.FirstVintageDate = firstVintageDateTask.Result;
        result.Item.LastVintageDate = lastVintageDateTask.Result;
        result.Item.MinValue = minValueTask.Result;
        result.Item.MaxValue = maxValueTask.Result;
        result.Success = true;
        logger.LogDebug("{m} complete.", nameof(GetSeriesStatistics));
        return result;
    }
}
