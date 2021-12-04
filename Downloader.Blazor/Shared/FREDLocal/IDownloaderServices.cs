namespace Downloader.Blazor.Shared.FREDLocal;

public interface IDownloaderServices
{
    IObservationsService ObservationsService { get; }
    ISeriesService SeriesService { get; }
}
