using LeaderAnalytics.Core;
namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class CategoriesService : BaseService, ICategoriesService
{
    public CategoriesService(FREDStagingDb db, IAPI_Manifest downloaderServices, IFredClient fredClient, ILogger<CategoriesService> logger, Action<string> statusCallback) : base(db, downloaderServices, fredClient, logger, statusCallback)
    {

    }

    public async Task<RowOpResult> DownloadCategory(string categoryID, CancellationToken? cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadCategory), categoryID);
        Status($"Downloading Category {categoryID}.");
        RowOpResult result = new RowOpResult();

        if(cancellationToken?.IsCancellationRequested ?? false)
            return result;

        FredCategory category = await fredClient.GetCategory(categoryID);
        
        if (category != null)
            result = await SaveCategory(category);

        logger.LogDebug("{m} complete.", nameof(DownloadCategory));
        return result;
        
    }

    public async Task<RowOpResult> DownloadCategoryChildren(string categoryID, CancellationToken? cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadCategoryChildren), categoryID);
        Status($"Downloading child categories for categoryID {categoryID}");
        RowOpResult result = new RowOpResult();

        if (cancellationToken?.IsCancellationRequested ?? false)
            return result;

        List<FredCategory> categories = await fredClient.GetCategoryChildren(categoryID);

        if (categories?.Any() ?? false)
        {
            foreach (FredCategory category in categories)
                result = await SaveCategory(category, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadCategoryChildren));
        return result;
    }

    public async Task<RowOpResult> DownloadRelatedCategories(string parentID, CancellationToken? cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(parentID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadRelatedCategories), parentID);
        Status($"Downloading related categories for categoryID {parentID}");
        RowOpResult result = new RowOpResult();

        if (cancellationToken?.IsCancellationRequested ?? false)
            return result;

        List<FredRelatedCategory> relatedCategories = await fredClient.GetRelatedCategories(parentID);

        if (relatedCategories?.Any() ?? false)
        {
            foreach (FredRelatedCategory r in relatedCategories)
                await SaveRelatedCategory(r, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }

        logger.LogDebug("{m} complete.", nameof(DownloadRelatedCategories));
        return result;
    }

    public async Task<RowOpResult> DownloadCategorySeries(string categoryID, CancellationToken? cancellationToken, bool includeDiscontinued = false)
    {
        ArgumentNullException.ThrowIfNull(categoryID);
        logger.LogDebug("Starting {m}. Parameters are {p1}, {p2}", nameof(DownloadCategorySeries), categoryID, includeDiscontinued);
        Status($"Downloading series for categoryID {categoryID}");
        RowOpResult result = new RowOpResult();

        if (cancellationToken?.IsCancellationRequested ?? false)
            return result;

        FredCategory? category = await db.Categories.FirstOrDefaultAsync(x => x.NativeID == categoryID);

        if (category is null)
        {
            RowOpResult categoryResult = await DownloadCategory(categoryID, cancellationToken);

            if (!categoryResult.Success)
                return categoryResult;
        }

        if (cancellationToken?.IsCancellationRequested ?? false)
            return result;
        
        List<FredSeries> series = await fredClient.GetSeriesForCategory(categoryID, includeDiscontinued);

        if (series?.Any() ?? false)
        {
            foreach (FredSeries s in series)
            {
                await serviceManifest.SeriesService.SaveSeries(s, false);
                await serviceManifest.SeriesService.SaveSeriesCategory(new FredSeriesCategory { Symbol = s.Symbol, CategoryID = categoryID }, false);
            }

            await db.SaveChangesAsync();
        }
        result.Success = true;
        logger.LogDebug("{m} complete.", nameof(DownloadCategorySeries));
        return result;
    }

    public async Task<RowOpResult> DownloadCategoriesForSeries(string symbol, CancellationToken? cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(symbol);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadCategoriesForSeries), symbol);
        Status($"Downloading categories for series {symbol}");
        RowOpResult seriesResult = await serviceManifest.SeriesService.DownloadSeriesIfItDoesNotExist(symbol, cancellationToken);

        if (!seriesResult.Success)
            return seriesResult;

        RowOpResult result = new RowOpResult();

        if (cancellationToken?.IsCancellationRequested ?? false)
            return result;

        List<FredCategory> categores = await fredClient.GetCategoriesForSeries(symbol);

        if (categores?.Any() ?? false)
        {
            foreach (FredCategory category in categores)
            {
                FredSeriesCategory seriesCategory = new FredSeriesCategory { Symbol = symbol, CategoryID = category.NativeID };
                await serviceManifest.SeriesService.SaveSeriesCategory(seriesCategory, false);
                await SaveCategory(category, false);
            }
            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadCategoriesForSeries));
        return result;
    }

    public async Task<RowOpResult> DownloadCategoryTags(string categoryID, CancellationToken? cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadCategoryTags), categoryID);
        Status($"Downloading tags for category {categoryID}");
        RowOpResult result = new RowOpResult();

        if (cancellationToken?.IsCancellationRequested ?? false)
            return result;

        List<FredCategoryTag> categoryTags = await fredClient.GetCategoryTags(categoryID);

        if (categoryTags?.Any() ?? false)
        {
            foreach(FredCategoryTag categoryTag in categoryTags)
                result = await SaveCategoryTag(categoryTag, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadCategoryTags));
        return result;
    }

    public async Task<RowOpResult> SaveCategory(FredCategory category, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(category);
        logger.LogDebug("Starting {m}. Parameters are {p1}, {p2}", nameof(SaveCategory), category, saveChanges);
        
        if (string.IsNullOrEmpty(category.NativeID))
            throw new Exception($"{nameof(category.NativeID)} is required.");
        else if(string.IsNullOrEmpty(category.Name))
            throw new Exception($"{nameof(category.Name)} is required.");

        RowOpResult result = new RowOpResult();

        if (category.NativeID == "0" && category.ParentID == "0")
        {
            // This is a placeholder returned by Fred.  Dont save it because it could cause recursion problems because id = parentID
            result.Success = true;
            return result;
        }

        FredCategory? dupe = db.Categories.FirstOrDefault(x => x.ID != category.ID && x.NativeID == category.NativeID);

        if (dupe is not null)
            result.Message = $"Duplicate with ID {dupe.ID} was found.";
        else
        {
            db.Entry(category).State = category.ID == 0 ? EntityState.Added : EntityState.Modified;

            if (saveChanges)
                await db.SaveChangesAsync();

            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveCategory));
        return result;
    }

    public async Task<RowOpResult> SaveCategoryTag(FredCategoryTag categoryTag, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(categoryTag);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(SaveCategoryTag), categoryTag, saveChanges);

        if (string.IsNullOrEmpty(categoryTag.CategoryID))
            throw new Exception($"{nameof(categoryTag.CategoryID)} is required.");
        else if (string.IsNullOrEmpty(categoryTag.Name))
            throw new Exception($"{nameof(categoryTag.Name)} is required.");
        else if (string.IsNullOrEmpty(categoryTag.GroupID))
            throw new Exception($"{nameof(categoryTag.GroupID)} is required.");

        RowOpResult result = new RowOpResult();
        FredCategoryTag? dupe = db.CategoryTags.FirstOrDefault(x => x.ID != categoryTag.ID && x.Name == categoryTag.Name && x.GroupID == categoryTag.GroupID);

        if (dupe != null)
            result.Message = $"Duplicate with ID {dupe.ID} was found.";
        else
        {
            db.Entry(categoryTag).State = categoryTag.ID == 0 ? EntityState.Added : EntityState.Modified;

            if (saveChanges)
                await db.SaveChangesAsync();

            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveCategoryTag));
        return result;
    }

    public async Task<RowOpResult> SaveRelatedCategory(FredRelatedCategory category, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(category);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(SaveRelatedCategory), category, saveChanges);

        if (string.IsNullOrEmpty(category.CategoryID))
            throw new Exception($"{nameof(category.CategoryID)}  is required.");
        else if (string.IsNullOrEmpty(category.RelatedCategoryID))
            throw new Exception($"{nameof(category.RelatedCategoryID)}  is required.");

        RowOpResult result = new RowOpResult();
        FredRelatedCategory? dupe = db.RelatedCategories.FirstOrDefault(x => x.CategoryID == category.CategoryID && x.RelatedCategoryID == category.RelatedCategoryID);
        
        if (dupe is not null)
            result.Message = $"Duplicate with ID {dupe.ID} was found.";
        else
        {
            db.Entry(category).State = category.ID == 0 ? EntityState.Added : EntityState.Modified;

            if (saveChanges)
                await db.SaveChangesAsync();
            
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveRelatedCategory));
        return result;
    }

    public async Task<List<FredCategory>> GetLocalChildCategories(string? parentID, bool sortAscending = true, string searchExpression = null, int skip = 0, int take = int.MaxValue)
    {
        logger.LogDebug("Starting {m}. Parameters are {p1}, {p2}, {p3}, {p4}, {p5}", nameof(GetLocalChildCategories), parentID, sortAscending, searchExpression, skip, take);
        return await db.Categories
            .Where(x => x.ParentID == parentID && (string.IsNullOrEmpty(searchExpression) || x.Name.Contains(searchExpression)))
            .SortBy(x => x.Name, sortAscending)
            .Skip(skip).Take(take)
            .ToListAsync();
     
    }

    public async Task<List<FredCategory>> GetLocalCategoriesForSeries(string symbol)
    {
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(GetLocalCategoriesForSeries), symbol);

        var query = from sc in db.SeriesCategories 
                    join c in db.Categories on sc.CategoryID equals c.NativeID
                    where sc.Symbol == symbol
                    select c;

        logger.LogDebug("{m} complete.", nameof(GetLocalCategoriesForSeries));
        return await query.ToListAsync();
    }



    /// <summary>
    /// A Categroy Node is a collection of sub-categories and series for a parent node.
    /// </summary>
    /// <param name="categoryID"></param>
    /// <returns></returns>
    public async Task<List<Node>> GetCategoryNodes(string? categoryID, bool sortAscending = true, string titleSearchExpression = null, string symbolSearchExpression = null, int skip = 0, int take = int.MaxValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryID);
        logger.LogDebug("Starting {m}. Parameters are {p1}, {p2}, {p3}, {p4}, {p5}", nameof(GetCategoryNodes), categoryID, sortAscending, titleSearchExpression, skip, take);

        List<Node> nodes = new();
        List<FredCategory> categories = await GetLocalChildCategories(categoryID, sortAscending, titleSearchExpression);
        List<FredSeries> series = await serviceManifest.SeriesService.GetLocalSeriesForCategory(symbolSearchExpression, titleSearchExpression, categoryID, nameof(FredSeries.Title), sortAscending);
        
        foreach (FredCategory category in categories ?? Enumerable.Empty<FredCategory>()) 
            nodes.Add(new Node(category, categoryID));

        foreach (FredSeries s in series ?? Enumerable.Empty<FredSeries>())
            nodes.Add(new Node(s, categoryID) );

        logger.LogDebug("{m} complete.", nameof(GetCategoryNodes));
        return nodes.Skip(skip).Take(take).ToList();
    }
}
