namespace Downloader.APIClient;

public abstract class BaseService
{
    protected readonly HttpClient httpClient;
    protected readonly IDownloaderServices serviceManifest;

    public BaseService(IDownloaderServices serviceManifest, IHttpClientFactory httpClientFactory)
    {
        this.serviceManifest = serviceManifest;
        this.httpClient = httpClientFactory.CreateClient("ServerAPI");
    }
}
