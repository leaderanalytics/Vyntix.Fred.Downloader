namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class DownloadService : BaseService, IDownloadService
{
    private FredDownloadArgs args;

    public DownloadService(FREDStagingDb db, IAPI_Manifest serviceManifest, IFredClient fredClient, ILogger<DownloadService> logger, Action<string> statusCallback) : base(db, serviceManifest, fredClient, logger, statusCallback)
    { 
        
    }

    public async Task<APIResult> Download(FredDownloadArgs args, CancellationToken? cancellationToken)
    {
        logger.LogDebug("Starting {m}. Parameters are {@p1}", nameof(DownloadService.Download), args);
        ArgumentNullException.ThrowIfNull(args);
        this.args = args;

        if (!string.IsNullOrEmpty(args.CategoryID))
            return await DownloadCategoryPath(args.CategoryID, cancellationToken);
        else if (args.Symbols?.Any() ?? false)
            return await DownloadSymbolPath(args.Symbols, cancellationToken);
        else
            throw new Exception("A CategoryID or one or more symbols must be set on FredDownloadArgs.");
    }


    private async Task<APIResult> DownloadSymbolPath(string[] symbols, CancellationToken? cancellationToken)
    {
        logger.LogDebug("Starting {m}. Parameters are {@p1}", nameof(DownloadService.DownloadSymbolPath), symbols);
           
        APIResult result = new();

        if(symbols.Count() == 1 && symbols.First() == "*") // Update all symbols in local db
            symbols = (await serviceManifest.SeriesService.GetLocalSeries()).Select(x => x.Symbol).ToArray();


        // It does not make sense to download any object further down the hierarchy if
        // series does not exist.  Always download series.
        
        await serviceManifest.SeriesService.DownloadSeries(symbols, cancellationToken);

        await DownloadSymbolBasedObjects(symbols, cancellationToken);

        if (args.SeriesCategories)
        {
            foreach (string symbol in symbols)
            {
                if (cancellationToken?.IsCancellationRequested ?? false)
                    break;

                await serviceManifest.CategoriesService.DownloadCategoriesForSeries(symbol, cancellationToken);
                List<FredCategory> categories = await serviceManifest.CategoriesService.GetLocalCategoriesForSeries(symbol);

                if (args.ChildCategories)
                {
                    foreach (FredCategory category in categories)
                    {
                        if (cancellationToken?.IsCancellationRequested ?? false)
                            break;

                        await serviceManifest.CategoriesService.DownloadCategoryChildren(category.NativeID, cancellationToken);
                    }
                }

                if (args.RelatedCategories)
                {   
                    foreach (FredCategory category in categories)
                    {
                        if (cancellationToken?.IsCancellationRequested ?? false)
                            break;

                        await serviceManifest.CategoriesService.DownloadRelatedCategories(category.NativeID, cancellationToken);
                    }
                }

                if(args.CategoryTags)
                {    
                    foreach (FredCategory category in categories)
                    {
                        if (cancellationToken?.IsCancellationRequested ?? false)
                            break;

                        await serviceManifest.CategoriesService.DownloadCategoryTags(category.NativeID, cancellationToken);
                    }
                }
            }
        }
        await db.SaveChangesAsync();
        result.Success = true;
        logger.LogDebug("{m} complete.", nameof(DownloadService.DownloadSymbolPath));
        return result;
    }


    private async Task<APIResult> DownloadCategoryPath(string categoryID, CancellationToken? cancellationToken)
    {
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadService.DownloadCategoryPath), categoryID);
        APIResult result = new();

        if (args.Series || args.SeriesTags || args.Releases || args.ReleaseDates || args.Sources || args.Observations)
        {
            // It does not make sense to download any object further down the hierarchy if
            // series does not exist.  Always download series.
            await serviceManifest.CategoriesService.DownloadCategorySeries(categoryID, cancellationToken, args.IncludeDiscontinuedSeries);

            // Get symbols for every series in the category
            string[] symbols = ((await serviceManifest.SeriesService.GetLocalSeriesForCategory(null,null,categoryID))?.Select(x => x.Symbol) ?? Enumerable.Empty<string>()).ToArray();

            if (!(cancellationToken?.IsCancellationRequested ?? false))
                await DownloadSymbolBasedObjects(symbols, cancellationToken);
        }
        
        if(args.ChildCategories)
            await serviceManifest.CategoriesService.DownloadCategoryChildren(categoryID, cancellationToken);

        if (args.RelatedCategories)
            await serviceManifest.CategoriesService.DownloadRelatedCategories(categoryID, cancellationToken);

        if (args.CategoryTags)
                await serviceManifest.CategoriesService.DownloadCategoryTags(categoryID, cancellationToken);

        await db.SaveChangesAsync();

        if (args.Recurse && args.ChildCategories && !(cancellationToken?.IsCancellationRequested ?? false))
        {
            List<FredCategory> childCategories = await serviceManifest.CategoriesService.GetLocalChildCategories(categoryID);

            foreach (FredCategory childCategory in childCategories)
            {
                await DownloadCategoryPath(childCategory.NativeID, cancellationToken);

                if (cancellationToken?.IsCancellationRequested ?? false)
                    break;
            }
        }
        result.Success = true;
        logger.LogDebug("{m} complete.", nameof(DownloadService.DownloadCategoryPath));
        return result;
    }


    private async Task DownloadSymbolBasedObjects(string[] symbols, CancellationToken? cancellationToken)
    {
        logger.LogDebug("Starting {m}. Parameters are {@p1}", nameof(DownloadService.DownloadSymbolBasedObjects), symbols);
        // If called by DownloadSymbolPath, symbols are an arbitrary list input by the user.
        // If called by DownloadCategoryPath, symbols are all series within a category.

        if (args.Observations && !(cancellationToken?.IsCancellationRequested ?? false))
            await serviceManifest.ObservationsService.DownloadObservations(symbols, cancellationToken);

        if (args.SeriesTags && !(cancellationToken?.IsCancellationRequested ?? false))
            foreach (string symbol in symbols)
                await serviceManifest.SeriesService.DownloadSeriesTags(symbol, cancellationToken);

        if ((args.Releases || args.Sources || args.ReleaseDates) && !(cancellationToken?.IsCancellationRequested ?? false))
        {
            foreach (string symbol in symbols)
            {
                await serviceManifest.SeriesService.DownloadSeriesRelease(symbol, cancellationToken);
                
                if ((cancellationToken?.IsCancellationRequested ?? false))
                    break;
            }
        }


        if (args.Sources || args.ReleaseDates && !(cancellationToken?.IsCancellationRequested ?? false))
        {
            foreach (string symbol in symbols)
            {
                FredSeries? series = (await serviceManifest.SeriesService.GetLocalSeries(symbol)).FirstOrDefault();

                if (series is not null && !string.IsNullOrEmpty(series.ReleaseID))
                {
                    if (args.Sources)
                        await serviceManifest.ReleasesService.DownloadReleaseSources(series.ReleaseID, cancellationToken);

                    if (args.ReleaseDates)
                        await serviceManifest.ReleasesService.DownloadReleaseDates(series.ReleaseID, cancellationToken);
                }

                if ((cancellationToken?.IsCancellationRequested ?? false))
                    break;
            }
        }
        logger.LogDebug("{m} complete.", nameof(DownloadService.DownloadSymbolBasedObjects));
    }
}
