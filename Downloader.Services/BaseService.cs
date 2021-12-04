namespace Downloader.Services;

public abstract class BaseService
{
    protected readonly Db db;
    protected readonly IFredClient fredClient;
    protected readonly IDownloaderServices serviceManifest;

    public BaseService(Db db, IDownloaderServices serviceManifest, IFredClient fredClient)
    {
        this.db = db ?? throw new ArgumentNullException(nameof(db));
        this.fredClient = fredClient ?? throw new ArgumentNullException(nameof(fredClient));
        this.serviceManifest = serviceManifest ?? throw new ArgumentNullException(nameof(serviceManifest));
    }
}
