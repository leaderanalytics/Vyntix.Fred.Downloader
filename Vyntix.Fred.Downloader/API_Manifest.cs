namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class API_Manifest : ServiceManifestFactory, IAPI_Manifest
{
    public IObservationsService ObservationsService => Create<IObservationsService>();
    public IReleasesService ReleasesService => Create<IReleasesService>();
    public ISeriesService SeriesService => Create<ISeriesService>();
    public ICategoriesService CategoriesService => Create<ICategoriesService>();
}
