﻿using LeaderAnalytics.Vyntix.Fred.Model;
using System.Runtime.InteropServices;

namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class SeriesService : BaseService, ISeriesService
{
    public SeriesService(FREDStagingDb db, IAPI_Manifest serviceManifest,  IFredClient fredClient) : base(db, serviceManifest, fredClient)
    {

    }

    public async Task<List<RowOpResult>> DownloadSeries(string[] symbols, string? releaseID = null)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        List<RowOpResult> results = new();

        foreach (string symbol in symbols)
        {
            RowOpResult result = await DownloadSeries(symbol, releaseID);
            
            if (result.Success)
                result.Message = symbol;

            results.Add(result);
        }

        return results;
    }


    public async Task<RowOpResult> DownloadSeries(string symbol, string? releaseID = null)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(symbol);
        FredSeries series = await fredClient.GetSeries(symbol);
        RowOpResult result = new RowOpResult();

        if (series is null)
            result.Message = $"Series with symbol {symbol} was not found.";
        else
        {
            series.ReleaseID = releaseID;
            series.LastMetadataCheck = DateTime.Now.ToUniversalTime(); // Don't put this in SaveSeries because we may save a series after user edits - which may not include an update check from FRED.
            result = await SaveSeries(series, true);
            db.Entry(series).State = EntityState.Detached; // Because in DownloadSeriesIfItDoesNotExist we get this entity again.  See ObservationsServiceTests.cs
        }
        return result;
    }

    public async Task<RowOpResult> DownloadCategoriesForSeries(string symbol)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(symbol);
        RowOpResult seriesResult = await DownloadSeriesIfItDoesNotExist(symbol);
        
        if (!seriesResult.Success)
            return seriesResult;
        
        RowOpResult result = new RowOpResult();
        List<FredCategory> categores = await fredClient.GetCategoriesForSeries(symbol);
        
        if (categores?.Any() ?? false)
        {
            foreach (FredCategory category in categores)
            {
                FredSeriesCategory seriesCategory = new FredSeriesCategory { Symbol = symbol, CategoryID = category.NativeID };
                await SaveSeriesCategory(seriesCategory, false);
            }
            await db.SaveChangesAsync();
            result.Success = true;
        }
        return result;
    }

    public async Task<RowOpResult> DownloadSeriesRelease(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult result = new RowOpResult();
        FredRelease? release = await fredClient.GetReleaseForSeries(symbol);
        
        if (release is null)
        {
            result.Message = $"Release not found for series {symbol}";
            return result;
        }

        FredSeries? series = db.Series.FirstOrDefault(x => x.Symbol == symbol);

        if (series is null)
        {
            RowOpResult seriesResult = await DownloadSeries(symbol, release.NativeID);

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
        return result;
    }

    public async Task<RowOpResult> DownloadSeriesTags(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult result = new();
        List<FredSeriesTag> seriesTags = await fredClient.GetSeriesTags(symbol);

        if (seriesTags?.Any() ?? false)
        {
            foreach (FredSeriesTag seriesTag in seriesTags)
                result = await SaveSeriesTag(seriesTag, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        return result;
    }

    public async Task<RowOpResult> DeleteSeriesTagsForSymbol(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult result = new();
        await db.SeriesTags.Where(x => x.Symbol == symbol).ExecuteDeleteAsync();
        result.Success = true;
        return result;
    }

    public async Task<RowOpResult<FredSeries>> DownloadSeriesIfItDoesNotExist(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult<FredSeries> result = new();
        result.Item = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);

        if (result.Item is null)
        {
            await DownloadSeries(symbol);
            result.Item = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol); // Needs to be detached
        }
        result.Success = result.Item is not null;
        return result;
    }

    public async Task<RowOpResult> SaveSeries(FredSeries series, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(series);
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
        return result;
    }

    public async Task<RowOpResult> SaveSeriesCategory(FredSeriesCategory seriesCategory, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(seriesCategory);
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
        
        return result;
    }

    public async Task<RowOpResult> DeleteSeriesCategoriesForSymbol(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult result = new();
        await db.SeriesCategories.Where(x => x.Symbol == symbol).ExecuteDeleteAsync();
        result.Success = true;
        return result;
    }

    public async Task<RowOpResult> SaveSeriesTag(FredSeriesTag seriesTag, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(seriesTag);

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
        return result;
    }

    public async Task<RowOpResult<List<FredSeries>>> GetLocalSeries(int skip, int take, string searchTitle)
    {
        RowOpResult<List<FredSeries>> result = new();
        result.Item = await db.Series
            .Where(x => string.IsNullOrEmpty(searchTitle) || x.Title.Contains(searchTitle))
            .OrderBy(x => x.Symbol).Skip(skip).Take(take).ToListAsync();

        result.Success = true;
        return result;
    }

    public async Task<RowOpResult<FredSeries>> GetLocalSeries(string symbol)
    {
        RowOpResult<FredSeries> result = new();
        result.Item = await db.Series.FirstOrDefaultAsync(x => x.Symbol == symbol);
        result.Success = result.Item is not null;
        return result;
    }



    public async Task<int> GetSeriesCount() => await db.Series.CountAsync();

    public async Task<RowOpResult> DeleteSeries(string symbol)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        RowOpResult result = new();
        await DeleteSeriesCategoriesForSymbol(symbol);
        await DeleteSeriesTagsForSymbol(symbol);
        await serviceManifest.ObservationsService.DeleteObservationsForSymbol(symbol);
        await db.Series.Where(x => x.Symbol == symbol).ExecuteDeleteAsync();
        result.Success = true;
        return result;
    }
}
