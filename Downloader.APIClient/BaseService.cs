using Downloader.Blazor.Shared.FREDLocal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Downloader.APIClient
{
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
}
