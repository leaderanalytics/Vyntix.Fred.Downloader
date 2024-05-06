namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class API_Manifest : ServiceManifestFactory, IAPI_Manifest
{
    public IDownloadService DownloadService => Create<IDownloadService>();
    public IObservationsService ObservationsService => Create<IObservationsService>();
    public IReleasesService ReleasesService => Create<IReleasesService>();
    public ISeriesService SeriesService => Create<ISeriesService>();
    public ICategoriesService CategoriesService => Create<ICategoriesService>();
    public IAuthenticationService AuthenticationService => Create<IAuthenticationService>();
}
