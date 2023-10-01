namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class ReleasesService : BaseService, IReleasesService
{
    public ReleasesService(FREDStagingDb db, IObserverAPI_Manifest serviceManifest, IFredClient fredClient) : base(db, serviceManifest, fredClient)
    {

    }

    public async Task<RowOpResult> DownloadAllReleases()
    {
        RowOpResult result = new RowOpResult();
        List<FredRelease> releases = await fredClient.GetAllReleases();

        if (releases?.Any() ?? false)
        {
            foreach (FredRelease release in releases)
                await SaveRelease(release, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        return result;
    }

    public async Task<RowOpResult> DownloadAllReleaseDates()
    {
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
        return result;
    }

    


    public async Task<RowOpResult> DownloadRelease(string releaseID)
    {
        ArgumentException.ThrowIfNullOrEmpty(releaseID);
        RowOpResult result = new RowOpResult();
        FredRelease? release = await fredClient.GetRelease(releaseID);

        if (release is not null)
            result = await SaveRelease(release, true);

        return result;
    }

    public async Task<RowOpResult> DownloadReleaseDates(string releaseID)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(releaseID);
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
        return result;
    }

    public async Task<RowOpResult> DownloadReleaseSeries(string releaseID)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(releaseID);
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
        return result;
    }

    public async Task<RowOpResult> DownloadReleaseSources(string releaseID)
    {
        ArgumentException.ThrowIfNullOrEmpty(releaseID);
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
        return result;
    }

    public async Task<RowOpResult> DownloadSourceReleases(string sourceID)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceID);
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
        return result;
    }

    public async Task<RowOpResult> DownloadAllSources()
    {
        RowOpResult result = new RowOpResult();
        List<FredSource> sources = await fredClient.GetSources();

        if (sources?.Any() ?? false)
        {
            foreach (FredSource source in sources)
                await SaveSource(source, false);

            await db.SaveChangesAsync();
            result.Success = true;
        }
        return result;
    }

    public async Task<RowOpResult> DownloadSource(string sourceID)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceID); 
        RowOpResult result = new RowOpResult();
        FredSource? source = await fredClient.GetSource(sourceID);

        if (source is not null)
        {
            result = await SaveSource(source, true);
            result.Success = true;
        }
        return result;
    }

    public async Task<RowOpResult> SaveRelease(FredRelease release, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(release);

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
        return result;
    }

    public async Task<RowOpResult> SaveReleaseDate(FredReleaseDate releaseDate, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(releaseDate);

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
        return result;
    }

    public async Task<RowOpResult> SaveSource(FredSource source, bool saveChanges = true)
    {
        ArgumentNullException.ThrowIfNull(source);

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
        return result;
    }

    private async Task DownloadReleaseIfItDoesNotExist(string releaseID)
    {
        if ((await db.Releases.FirstOrDefaultAsync(x => x.NativeID == releaseID)) is null)
            if (!(await DownloadRelease(releaseID)).Success)
                throw new Exception($"Invalid releaseID: {releaseID}");
    }

    private async Task DownloadSourceIfItDoesNotExist(string sourceID)
    {
        if ((await db.Sources.FirstOrDefaultAsync(x => x.NativeID == sourceID)) is null)
            if (!(await DownloadSource(sourceID)).Success)
                throw new Exception($"Invalid sourceID: {sourceID}");
    }
}
