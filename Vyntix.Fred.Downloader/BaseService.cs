namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public abstract class BaseService
{
    protected readonly FREDStagingDb db;
    protected readonly IFredClient fredClient;
    protected readonly IAPI_Manifest serviceManifest;

    public BaseService(FREDStagingDb db, IAPI_Manifest serviceManifest, IFredClient fredClient)
    {
        this.db = db ?? throw new ArgumentNullException(nameof(db));
        this.fredClient = fredClient ?? throw new ArgumentNullException(nameof(fredClient));
        this.serviceManifest = serviceManifest ?? throw new ArgumentNullException(nameof(serviceManifest));
    }
}
