namespace Downloader.Shared;

public class ServiceManifest : ServiceManifestFactory, IDownloaderServices
{
    public IObservationsService ObservationsService => Create<IObservationsService>();
    public ISeriesService SeriesService => Create<ISeriesService>();
}
