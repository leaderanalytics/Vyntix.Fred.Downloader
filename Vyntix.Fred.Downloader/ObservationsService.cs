using System.Collections;

namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class ObservationsService : BaseService, IObservationsService
{
    public ObservationsService(FREDStagingDb db, IObserverAPI_Manifest downloaderServices, IFredClient fredClient) : base(db, downloaderServices, fredClient)
    {
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
}
