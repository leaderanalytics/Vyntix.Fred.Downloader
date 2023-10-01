namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class CategoriesService : BaseService, ICategoriesService
{
    public CategoriesService(FREDStagingDb db, IObserverAPI_Manifest downloaderServices, IFredClient fredClient) : base(db, downloaderServices, fredClient)
    {

    }

    public async Task<RowOpResult> DownloadCategory(string categoryID)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryID);   
        RowOpResult result = new RowOpResult();
        FredCategory category = await fredClient.GetCategory(categoryID);
        
        if (category != null)
            result = await SaveCategory(category);

        return result;
    }

    public async Task<RowOpResult> DownloadCategoryChildren(string categoryID)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryID);
        RowOpResult result = new RowOpResult();
        List<FredCategory> categories = await fredClient.GetCategoryChildren(categoryID);

        if (categories?.Any() ?? false)
        {
            foreach (FredCategory category in categories)
                result = await SaveCategory(category, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        return result;
    }

    public async Task<RowOpResult> DownloadRelatedCategories(string parentID)
    {
        ArgumentException.ThrowIfNullOrEmpty(parentID);
        RowOpResult result = new RowOpResult();
        List<FredRelatedCategory> relatedCategories = await fredClient.GetRelatedCategories(parentID);

        if (relatedCategories?.Any() ?? false)
        {
            foreach (FredRelatedCategory r in relatedCategories)
                await SaveRelatedCategory(r, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        return result;
    }

    public async Task<RowOpResult> DownloadCategorySeries(string categoryID)
    {
        ArgumentNullException.ThrowIfNull(categoryID);
        RowOpResult result = new RowOpResult();
        FredCategory? category = await db.Categories.FirstOrDefaultAsync(x => x.NativeID == categoryID);

        if (category is null)
        {
            RowOpResult categoryResult = await DownloadCategory(categoryID);

            if (!categoryResult.Success)
                return categoryResult;
        }

        List<FredSeries> series = await fredClient.GetSeriesForCategory(categoryID, true);

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
        return result;
    }

    public async Task<RowOpResult> DownloadCategoryTags(string categoryID)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryID);
        RowOpResult result = new RowOpResult();
        List<FredCategoryTag> categoryTags = await fredClient.GetCategoryTags(categoryID);

        if (categoryTags?.Any() ?? false)
        {
            foreach(FredCategoryTag categoryTag in categoryTags)
                result = await SaveCategoryTag(categoryTag, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        return result;
    }

    public async Task<RowOpResult> SaveCategory(FredCategory category, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(category);
        
        if (string.IsNullOrEmpty(category.NativeID))
            throw new Exception($"{nameof(category.NativeID)} is required.");
        else if(string.IsNullOrEmpty(category.Name))
            throw new Exception($"{nameof(category.Name)} is required.");

        RowOpResult result = new RowOpResult();
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
        return result;
    }

    public async Task<RowOpResult> SaveCategoryTag(FredCategoryTag categoryTag, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(categoryTag);

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
        return result;
    }

    public async Task<RowOpResult> SaveRelatedCategory(FredRelatedCategory category, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(category);

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
     
        return result;
    }
}
