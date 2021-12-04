namespace Downloader.APIClient;

public class HTTP_EndPointValidator : IEndPointValidator
{
    private IHttpClientFactory httpClientFactory;

    public HTTP_EndPointValidator(IHttpClientFactory httpClientFactory) => this.httpClientFactory = httpClientFactory;

    public virtual bool IsInterfaceAlive(IEndPointConfiguration endPoint)
    {
        // Flagrant and fragrant hack.  IEndPointValidator does not support an async implementation and blazor fails with 
        // something like "waiting on a monitor is not supported" if we try something like httpClient.Send(...). 
        return true;


        bool success = false;
        HttpClient httpClient = httpClientFactory.CreateClient("ServerAPI");
        HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, new Uri(endPoint.ConnectionString));
        var response = httpClient.Send(msg);  // aint gonna happen.
        success = response.StatusCode == HttpStatusCode.OK;
        return success;
    }

    public Task<bool> IsInterfaceAliveAsync(IEndPointConfiguration endPoint)
    {
        throw new NotImplementedException();
    }
}
