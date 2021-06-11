using Downloader.Blazor.Shared;
using Downloader.Blazor.Shared.FREDLocal;
using LeaderAnalytics.Vyntix.Fred.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Downloader.APIClient
{
    public class ObservationsService : BaseService, IObservationsService
    {

        public ObservationsService(IDownloaderServices serviceManifest, IHttpClientFactory httpClientFactory) : base(serviceManifest, httpClientFactory)
        { 

        }

        public async Task<List<Observation>> GetLocalObservations(string symbol, int skip, int take)
        {
            try
            {
                return (await httpClient.GetFromJsonAsync<IEnumerable<Observation>>($"DownloaderServices/GetLocalObservations/{symbol}/{skip}/{take}"))?.ToList();
            }
            catch (Exception ex)
            {
                string y = ex.Message;
                return null;
            }
        }


        public async Task<List<Observation>> GetLocalObservations(string symbol)
        {
            return (await httpClient.GetFromJsonAsync<IEnumerable<Observation>>($"DownloaderServices/GetLocalObservations/{symbol}"))?.ToList();
        }

        public async Task<RowOpResult> UpdateLocalObservations(string symbol)
        {
            return (await httpClient.GetFromJsonAsync<RowOpResult>($"DownloaderServices/UpdateLocalObservations/{symbol}"));
        }

        public async Task<RowOpResult> DeleteLocalObservations(string symbol)
        {
            return (await httpClient.GetFromJsonAsync<RowOpResult>($"DownloaderServices/DeleteLocalObservations/{symbol}"));
        }
    }
}
