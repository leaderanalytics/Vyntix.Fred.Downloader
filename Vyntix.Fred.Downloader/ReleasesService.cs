using Microsoft.Identity.Client;

namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class ReleasesService : BaseService, IReleasesService
{
    public ReleasesService(FREDStagingDb db, IAPI_Manifest serviceManifest, IFredClient fredClient, ILogger<ReleasesService> logger, Action<string> statusCallback) : base(db, serviceManifest, fredClient, logger, statusCallback)
    {

    }

    public async Task<RowOpResult> DownloadAllReleases()
    {
        logger.LogDebug("Starting {m}.", nameof(DownloadAllReleases));
        RowOpResult result = new RowOpResult();
        List<FredRelease> releases = await fredClient.GetAllReleases();
        
        if (releases?.Any() ?? false)
        {
            foreach (FredRelease release in releases)
                await SaveRelease(release, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadAllReleases));
        return result;
    }

    public async Task<RowOpResult> DownloadAllReleaseDates()
    {
        logger.LogDebug("Starting {m}.", nameof(DownloadAllReleaseDates));
        RowOpResult result = new RowOpResult();

        List<FredReleaseDate> dates = await fredClient.GetAllReleaseDates(null, true);

        if (dates?.Any() ?? false)
        {
            foreach (var grp in dates.GroupBy(x => x.ReleaseID))
            {
                await DownloadReleaseIfItDoesNotExist(grp.Key);

                foreach (FredReleaseDate releaseDate in grp)
                    await SaveReleaseDate(releaseDate, false);
            }
            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadAllReleaseDates));
        return result;
    }

    


    public async Task<RowOpResult> DownloadRelease(string releaseID)
    {
        ArgumentException.ThrowIfNullOrEmpty(releaseID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadRelease), releaseID);
        Status($"Downloading release for releaseID {releaseID}");
        RowOpResult result = new RowOpResult();
        FredRelease? release = await fredClient.GetRelease(releaseID);

        if (release is not null)
            result = await SaveRelease(release, true);

        logger.LogDebug("{m} complete.", nameof(DownloadRelease));
        return result;
    }

    public async Task<RowOpResult> DownloadReleaseDates(string releaseID)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(releaseID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadReleaseDates), releaseID);
        Status($"Downloading release dates for releaseID {releaseID}");
        RowOpResult result = new RowOpResult();
        await DownloadReleaseIfItDoesNotExist(releaseID);
        DateTime? maxDate = db.ReleaseDates.Where(x => x.ReleaseID == releaseID).Max(x => x == null ? null as DateTime? : x.DateReleased);
        List<FredReleaseDate> dates = await fredClient.GetReleaseDatesForRelease(releaseID, maxDate, true);

        if (dates?.Any() ?? false)
        {
            foreach (FredReleaseDate releaseDate in dates)
                await SaveReleaseDate(releaseDate, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadReleaseDates));
        return result;
    }

    public async Task<RowOpResult> DownloadReleaseSeries(string releaseID)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(releaseID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadReleaseSeries), releaseID);
        Status($"Downloading series for releaseID {releaseID}");
        RowOpResult result = new RowOpResult();
        await DownloadReleaseIfItDoesNotExist(releaseID);
        List<FredSeries> seriess = await fredClient.GetSeriesForRelease(releaseID);

        if (seriess?.Any() ?? false)
        {
            foreach (FredSeries series in seriess)
            {
                FredSeries? existing = await db.Series.FirstOrDefaultAsync(x => x.Symbol == series.Symbol);

                if (existing is not null)
                {
                    existing.ReleaseID = series.ReleaseID;
                    await serviceManifest.SeriesService.SaveSeries(existing, false);
                }
                else
                    await serviceManifest.SeriesService.SaveSeries(series, false);
            }
            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadReleaseSeries));
        return result;
    }

    public async Task<RowOpResult> DownloadReleaseForSeries(string symbol, bool saveChanges = true)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        logger.LogDebug("Starting {m}. Parameters are {p1}, {p1}", nameof(DownloadReleaseForSeries), symbol, saveChanges);
        Status($"Downloading release for symbol {symbol}");
        FredRelease release = await fredClient.GetReleaseForSeries(symbol);
        logger.LogDebug("{m} complete.", nameof(DownloadReleaseForSeries));
        return await SaveRelease(release, saveChanges);
    }

    public async Task<RowOpResult> DownloadReleaseSources(string releaseID)
    {
        ArgumentException.ThrowIfNullOrEmpty(releaseID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadReleaseSources), releaseID);
        Status($"Downloading release sources for releaseID {releaseID}");
        RowOpResult result = new RowOpResult();
        await DownloadReleaseIfItDoesNotExist(releaseID);
        List<FredSource> sources = await fredClient.GetSourcesForRelease(releaseID);

        if (sources?.Any() ?? false)
        {
            foreach (FredSource source in sources)
                await SaveSource(source, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadReleaseSources));
        return result;
    }

    public async Task<RowOpResult> DownloadSourceReleases(string sourceID)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadSourceReleases), sourceID);
        Status($"Downloading releases for sourceID {sourceID}");
        RowOpResult result = new RowOpResult();
        await DownloadSourceIfItDoesNotExist(sourceID);
        List<FredRelease> releases = await fredClient.GetReleasesForSource(sourceID);

        if (releases?.Any() ?? false)
        {
            foreach (FredRelease release in releases)
                await SaveRelease(release, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadSourceReleases));
        return result;
    }

    public async Task<RowOpResult> DownloadAllSources()
    {
        logger.LogDebug("Starting {m}.", nameof(DownloadAllSources));
        RowOpResult result = new RowOpResult();
        List<FredSource> sources = await fredClient.GetSources();

        if (sources?.Any() ?? false)
        {
            foreach (FredSource source in sources)
                await SaveSource(source, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadAllSources));
        return result;
    }

    public async Task<RowOpResult> DownloadSource(string sourceID)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceID);
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadSource), sourceID);
        Status($"Downloading source for sourceID {sourceID}");
        RowOpResult result = new RowOpResult();
        FredSource? source = await fredClient.GetSource(sourceID);

        if (source is not null)
        {
            result = await SaveSource(source, true);
            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(DownloadSource));
        return result;
    }

    public async Task<RowOpResult> SaveRelease(FredRelease release, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(release);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(SaveRelease), release, saveChanges);

        if (string.IsNullOrEmpty(release.NativeID))
            throw new Exception($"{nameof(release.NativeID)} is required.");
        else if (string.IsNullOrEmpty(release.Name))
            throw new Exception($"{nameof(release.Name)} is required.");

        RowOpResult result = new RowOpResult();
        FredRelease? dupe = db.Releases.FirstOrDefault(x => x.ID != release.ID && x.NativeID == release.NativeID);

        if (dupe is not null)
            result.Message = $"Duplicate with ID {dupe.ID} was found.";
        else
        {
            db.Entry(release).State = release.ID == 0 ? EntityState.Added : EntityState.Modified;

            if (release.SourceReleases?.Any() ?? false)
            {
                foreach (FredSourceRelease sr in release.SourceReleases)
                {
                    sr.ReleaseNativeID = release.NativeID; // safety check

                    // check for dupe
                    if (db.SourceReleases.Any(x => x.SourceNativeID == sr.SourceNativeID && x.ReleaseNativeID == sr.ReleaseNativeID))
                        continue;

                    // make sure the source exists
                    if (!db.Sources.Any(x => x.NativeID == sr.ReleaseNativeID))
                        continue;

                    db.Entry(sr).State = EntityState.Added;
                }
            }

            if (saveChanges)
                await db.SaveChangesAsync();

            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveRelease));
        return result;
    }

    public async Task<RowOpResult> SaveReleaseDate(FredReleaseDate releaseDate, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(releaseDate);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(SaveReleaseDate), releaseDate, saveChanges);
        

        if (string.IsNullOrEmpty(releaseDate.ReleaseID))
            throw new Exception($"{nameof(releaseDate.ReleaseID)} is required.");

        RowOpResult result = new RowOpResult();
        FredReleaseDate? dupe = db.ReleaseDates.FirstOrDefault(x => x.ID != releaseDate.ID && x.ReleaseID == releaseDate.ReleaseID && x.DateReleased == releaseDate.DateReleased);

        if (dupe is not null)
            result.Message = $"Duplicate with ID {dupe.ID} was found.";
        else
        {
            db.Entry(releaseDate).State = releaseDate.ID == 0 ? EntityState.Added : EntityState.Modified;

            if (saveChanges)
                await db.SaveChangesAsync();

            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveReleaseDate));
        return result;
    }

    public async Task<RowOpResult> SaveSource(FredSource source, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        logger.LogDebug("Starting {m}. Parameters are {@p1}, {p2}", nameof(SaveSource), source, saveChanges);

        if (string.IsNullOrEmpty(source.NativeID))
            throw new Exception($"{nameof(source.NativeID)} is required.");
        else if (string.IsNullOrEmpty(source.Name))
            throw new Exception($"{nameof(source.Name)} is required.");

        RowOpResult result = new RowOpResult();
        FredSource? dupe = db.Sources.FirstOrDefault(x => x.ID != source.ID && x.NativeID == source.NativeID);

        if (dupe is not null)
            result.Message = $"Duplicate with ID {dupe.ID} was found.";
        else
        {
            db.Entry(source).State = source.ID == 0 ? EntityState.Added : EntityState.Modified;

            if (source.SourceReleases?.Any() ?? false)
            {
                foreach (FredSourceRelease sr in source.SourceReleases)
                {
                    sr.SourceNativeID = source.NativeID; // safety check

                    // check for dupe
                    if (db.SourceReleases.Any(x => x.SourceNativeID == sr.SourceNativeID && x.ReleaseNativeID == sr.ReleaseNativeID))
                        continue;

                    // make sure the release exists
                    if (!db.Releases.Any(x => x.NativeID == sr.ReleaseNativeID))
                        continue;

                    db.Entry(sr).State = EntityState.Added;
                }
            }

            if (saveChanges)
                await db.SaveChangesAsync();

            result.Success = true;
        }
        logger.LogDebug("{m} complete.", nameof(SaveSource));
        return result;
    }

    private async Task DownloadReleaseIfItDoesNotExist(string releaseID)
    {
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadReleaseIfItDoesNotExist), releaseID);
        
        if ((await db.Releases.FirstOrDefaultAsync(x => x.NativeID == releaseID)) is null)
            if (!(await DownloadRelease(releaseID)).Success)
                throw new Exception($"Invalid releaseID: {releaseID}");

        logger.LogDebug("{m} complete.", nameof(DownloadReleaseIfItDoesNotExist));
    }

    private async Task DownloadSourceIfItDoesNotExist(string sourceID)
    {
        logger.LogDebug("Starting {m}. Parameters are {p1}", nameof(DownloadSourceIfItDoesNotExist), sourceID);

        if ((await db.Sources.FirstOrDefaultAsync(x => x.NativeID == sourceID)) is null)
            if (!(await DownloadSource(sourceID)).Success)
                throw new Exception($"Invalid sourceID: {sourceID}");

        logger.LogDebug("{m} complete.", nameof(DownloadSourceIfItDoesNotExist));
    }
}
