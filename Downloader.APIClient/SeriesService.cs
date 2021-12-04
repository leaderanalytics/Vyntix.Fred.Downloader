namespace Downloader.APIClient;

public class SeriesService : BaseService, ISeriesService
{

    public SeriesService(IDownloaderServices serviceManifest, IHttpClientFactory httpClientFactory) : base(serviceManifest, httpClientFactory)
    {

    }

    public async Task<IEnumerable<Series>> GetLocalSeries(int skip, int take, string title)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<Series>>($"DownloaderServices/GetLocalSeries/{skip}/{take}/{title}");
        }
        catch (Exception ex)
        {
            string y = ex.Message;
            return null;
        }
    }

    public async Task<RowOpResult> SaveLocalSeries(string symbol)
    {
        return (await httpClient.GetFromJsonAsync<RowOpResult>($"DownloaderServices/SaveLocalSeries/{symbol}"));
    }

    public async Task<RowOpResult> DeleteLocalSeries(string symbol)
    {
        //HttpRequestMessage request = new HttpRequestMessage
        //{
        //    Method = HttpMethod.Post,
        //    RequestUri = new Uri($"{httpClient.BaseAddress}DownloaderServices/DeleteLocalSeries"),
        //    Content = JsonContent.Create(symbols)
        //};

        //HttpResponseMessage response = await httpClient.SendAsync(request);
        //return await response.Content.ReadFromJsonAsync<RowOpResult[]>();

        return (await httpClient.GetFromJsonAsync<RowOpResult>($"DownloaderServices/DeleteLocalSeries/{symbol}"));
    }

    public async Task<IEnumerable<string>> GetLocalSeriesSymbols()
    {
        return (await httpClient.GetFromJsonAsync<IEnumerable<string>>($"DownloaderServices/GetLocalSeriesSymbols"));
    }
}
