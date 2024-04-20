using System.Threading;

namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class SeriesService : BaseService, ISeriesService
{
    public SeriesService(FREDStagingDb db, IAPI_Manifest serviceManifest, IFredClient fredClient, ILogger<SeriesService> logger, Action<string> statusCallback) : base(db, serviceManifest, fredClient, logger, statusCallback)
    {

    }

    public async Task<List<RowOpResult>> DownloadSeries(string[] symbols, CancellationToken? cancellationToken, string? releaseID = null)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(DownloadSeries), symbols, releaseID);
        
        List<RowOpResult> results = new();

        if (cancellationToken?.IsCancellationRequested ?? false)
            return results;

        foreach (string symbol in symbols)
        {
            RowOpResult result = await DownloadSeries(symbol, cancellationToken, releaseID);
            
            if (result.Success)
                result.Message = symbol;

            results.Add(result);

            if (cancellationToken?.IsCancellationRequested ?? false)
                break;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadSeries));
        return results;
    }


    public async Task<RowOpResult> DownloadSeries(string symbol, CancellationToken? cancellationToken, string? releaseID = null)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(symbol);
        logger.LogDebug("Starting {m}. Parameters are {p1}, {p2}", nameof(DownloadSeries), symbol, releaseID);
        Status($"Downloading series for symbol {symbol}");
        RowOpResult result = new RowOpResult();
        
        if (cancellationToken?.IsCancellationRequested ?? false)
            return result;

        FredSeries series = await fredClient.GetSeries(symbol);
        

        if (series is null)
            result.Message = $"Series with symbol {symbol} was not found.";
        else
        {
            series.ReleaseID = releaseID;
            series.LastMetadataCheck = DateTime.Now.ToUniversalTime(); // Don't put this in SaveSeries because we may save a series after user edits - which may not include an update check from FRED.
            result = await SaveSeries(series, true);
            db.Entry(series).State = EntityState.Detached; // Because in DownloadSeriesIfItDoesNotExist we get this entity again.  See ObservationsServiceTests.cs
        }
        logger.LogDebug("{m} complete.", nameof(DownloadSeries));
        return result;
    }

  

    public async Task<RowOpResult> DownloadSeriesRelease(string symbol, CancellationToken? cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadSeriesRelease), symbol);
        Status($"Downloading release for series symbol {symbol}");
        RowOpResult result = new RowOpResult();

        if (cancellationToken?.IsCancellationRequested ?? false)
            return result;

        FredRelease? release = await fredClient.GetReleaseForSeries(symbol);
        
        if (release is null)
        {
            result.Message = $"Release not found for series {symbol}";
            return result;
        }

        FredSeries? series = (await DownloadSeriesIfItDoesNotExist(symbol, cancellationToken)).Item;

        if (series is null)
        {
            RowOpResult seriesResult = await DownloadSeries(symbol, cancellationToken, release.NativeID);

            if (!seriesResult.Success)
                return seriesResult;
        }
        else
        {
            series.ReleaseID = release.NativeID;
            await SaveSeries(series, true);
        }

        // release might already exist in which case we will get a dupe error.  Ignore it.
        await serviceManifest.ReleasesService.SaveRelease(release, true);
        result.Success = true;
        logger.LogDebug("{m} complete.", nameof(DownloadSeriesRelease));
        return result;
    }

    public async Task<RowOpResult> DownloadSeriesTags(string symbol, CancellationToken? cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadSeries), symbol);
        Status($"Downloading tags for series symbol {symbol}");
        RowOpResult result = new();
        List<FredSeriesTag> seriesTags = await fredClient.GetSeriesTags(symbol);

        if (seriesTags?.Any() ?? false)
        {
            foreach (FredSeriesTag seriesTag in seriesTags)
                result = await SaveSeriesTag(seriesTag, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadSeriesTags));
        return result;
    }

    public async Task<RowOpResult> DeleteSeriesTagsForSymbol(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        logger.LogInformation("Starting {m}. Parameters are {p1}", nameof(DeleteSeriesTagsForSymbol), symbol);
        Status($"Deleting series tags for symbol {symbol}");
        RowOpResult result = new();
        await db.SeriesTags.Where(x => x.Symbol == symbol).ExecuteDeleteAsync();
        result.Success = true;
        logger.LogInformation("{m} complete.", nameof(DeleteSeriesCategoriesForSymbol));
        return result;
    }

    public async Task<RowOpResult<FredSeries>> DownloadSeriesIfItDoesNotExist(string symbol, CancellationToken? cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadSeriesIfItDoesNotExist), symbol);
        RowOpResult<FredSeries> result = new();
        int? id = (await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol))?.ID;
        
        if (id.HasValue)
            result.Item = await db.Series.FindAsync(id);

        if (result.Item is null)
        {
            await DownloadSeries(symbol, cancellationToken);
            id = (await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol))?.ID;
            result.Item = await db.Series.FindAsync(id);
        }
        result.Success = result.Item is not null;
        logger.LogDebug("{m} complete.", nameof(DownloadSeriesIfItDoesNotExist));
        return result;
    }

    public async Task<RowOpResult> SaveSeries(FredSeries series, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(series);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(SaveSeries), series, saveChanges);
        RowOpResult result = new RowOpResult();
        
        if(string.IsNullOrEmpty(series.Symbol))
            throw new Exception($"{nameof(series.Symbol)} is required.");

        FredSeries? dupe = await db.Series.FirstOrDefaultAsync(x => x.ID != series.ID && x.Symbol == series.Symbol );

        if (dupe is not null)
            result.Message = $"Duplicate with ID {dupe.ID} for symbol {series.Symbol} was found.";
        else
        {
            db.Entry(series).State = series.ID == 0 ? EntityState.Added : EntityState.Modified;

            if (saveChanges)
                await db.SaveChangesAsync();

            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveSeries));
        return result;
    }

    public async Task<RowOpResult> SaveSeriesCategory(FredSeriesCategory seriesCategory, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(seriesCategory);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(SaveSeriesCategory), seriesCategory, saveChanges);
        RowOpResult result = new RowOpResult();

        if (String.IsNullOrEmpty(seriesCategory.CategoryID))
            throw new Exception($"{nameof(FredSeriesCategory.CategoryID)} is required.");
        else if (string.IsNullOrEmpty(seriesCategory.Symbol))
            throw new Exception($"{nameof(FredSeriesCategory.Symbol)} is required.");

        FredSeriesCategory? dupe = await db.SeriesCategories.FirstOrDefaultAsync(x => x.ID != seriesCategory.ID  && seriesCategory.CategoryID == x.CategoryID && x.Symbol == seriesCategory.Symbol);

        if (dupe is not null)
            result.Message = $"Duplicate with ID {dupe.ID} was found.";
        else
        {
            db.Entry(seriesCategory).State = seriesCategory.ID == 0 ? EntityState.Added : EntityState.Modified;
            
            if (saveChanges)
                await db.SaveChangesAsync();
            
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveSeriesCategory));
        return result;
    }

    public async Task<RowOpResult> DeleteSeriesCategoriesForSymbol(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        logger.LogInformation("Starting {m}. Parameters are {p1}", nameof(DeleteSeriesCategoriesForSymbol), symbol);
        RowOpResult result = new();
        await db.SeriesCategories.Where(x => x.Symbol == symbol).ExecuteDeleteAsync();
        result.Success = true;
        logger.LogInformation("{m} complete.", nameof(DeleteSeriesCategoriesForSymbol));
        return result;
    }

    public async Task<RowOpResult> SaveSeriesTag(FredSeriesTag seriesTag, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(seriesTag);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(SaveSeriesTag), seriesTag, saveChanges);

        if (string.IsNullOrEmpty(seriesTag.Symbol))
            throw new Exception($"{nameof(seriesTag.Symbol)} is required.");
        else if (string.IsNullOrEmpty(seriesTag.Name))
            throw new Exception($"{nameof(seriesTag.Name)} is required.");
        else if (string.IsNullOrEmpty(seriesTag.GroupID))
            throw new Exception($"{nameof(seriesTag.GroupID)} is required.");

        RowOpResult result = new RowOpResult();
        FredSeriesTag? dupe = db.SeriesTags.FirstOrDefault(x => x.ID != seriesTag.ID && x.Name == seriesTag.Name && x.GroupID == seriesTag.GroupID);

        if (dupe != null)
            result.Message = $"Duplicate with ID {dupe.ID} was found.";
        else
        {
            db.Entry(seriesTag).State = seriesTag.ID == 0 ? EntityState.Added : EntityState.Modified;

            if (saveChanges)
                await db.SaveChangesAsync();

            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveSeriesTag));
        return result;
    }

    public async Task<List<FredSeries>> GetLocalSeries(string? searchSymbol = null, string? searchTitle = null, string? sortExpression = null, bool sortAscending = true, int skip = 0, int take = int.MaxValue)
    {
        logger.LogDebug("Starting {m}. Parameters are {p1}, {p2}, {p3}, {p4}, {p5}, {p6}", nameof(GetLocalSeries), searchSymbol, searchTitle, sortExpression, sortAscending, skip, take);

        var query = db.Series
            .Where(x => (string.IsNullOrEmpty(searchTitle) || x.Title.Contains(searchTitle)) && (string.IsNullOrEmpty(searchSymbol) || x.Symbol.Contains(searchSymbol)));

        query = sortExpression switch
        {
            nameof(FredSeries.Title) => query.SortBy(x => x.Title, sortAscending),
            nameof(FredSeries.Frequency) => query.SortBy(x => x.Frequency, sortAscending),
            nameof(FredSeries.Units) => query.SortBy(x => x.Units, sortAscending),
            nameof(FredSeries.SeasonalAdj) => query.SortBy(x => x.SeasonalAdj, sortAscending),
            nameof(FredSeries.LastObsCheck) => query.SortBy(x => x.LastObsCheck, sortAscending),
            _ => query.SortBy(x => x.Symbol, sortAscending)
        };

        List<FredSeries> result = await query.Skip(skip).Take(take).ToListAsync();
        logger.LogDebug("{m} complete.", nameof(GetLocalSeries));
        return result;
    }

    public async Task<List<FredSeries>> GetLocalSeriesForCategory(string? symbol = null, string? titleSearchExpression = null,  string? categoryID = null, string? sortExpression = null,  bool sortAscending = true,  int skip = 0, int take = int.MaxValue)
    {
        logger.LogDebug("Starting {m}. Parameters are {p1}, {p2}, {p3}, {p4}, {p5}, {p6}", nameof(GetLocalSeriesForCategory), titleSearchExpression, categoryID, sortExpression, sortAscending, skip, take);

        var query = from sc in db.SeriesCategories
                    join s in db.Series on sc.Symbol equals s.Symbol
                    join c in db.Categories on sc.CategoryID equals c.NativeID
                    where 
                        (string.IsNullOrEmpty(symbol) || s.Symbol.Contains(symbol))
                        && (string.IsNullOrEmpty(categoryID) || sc.CategoryID == categoryID)
                        && (string.IsNullOrEmpty(titleSearchExpression) || s.Title.Contains(titleSearchExpression))
                    select s;


        query = sortExpression switch
        {
            nameof(FredSeries.Symbol) => query.SortBy(x => x.Symbol, sortAscending),
            _ => query.SortBy(x => x.Title, sortAscending)
        };
        query = query.Skip(skip).Take(take);
        List<FredSeries> result = await query.ToListAsync();
        logger.LogDebug("{m} complete.", nameof(GetLocalSeriesForCategory));
        return result;
    }

    public async Task<int> GetSeriesCount() => await db.Series.CountAsync();

    public async Task<RowOpResult> DeleteSeries(string symbol)
    {
        logger.LogInformation("Starting {m}. Parameters are {p1}", nameof(DeleteSeries), symbol); // Always log

        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult result = new();
        await DeleteSeriesCategoriesForSymbol(symbol);
        await DeleteSeriesTagsForSymbol(symbol);
        await serviceManifest.ObservationsService.DeleteObservationsForSymbol(symbol);
        await db.Series.Where(x => x.Symbol == symbol).ExecuteDeleteAsync();
        result.Success = true;

        logger.LogInformation("{m} complete.", nameof(DeleteSeries));
        
        return result;
    }
}
