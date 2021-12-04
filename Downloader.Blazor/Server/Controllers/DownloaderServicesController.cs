using Downloader.Blazor.Shared;
using Downloader.Blazor.Shared.FREDLocal;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.Vyntix.Fred.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Downloader.Blazor.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DownloaderServicesController : ControllerBase
    {
        private readonly IAdaptiveClient<IDownloaderServices> serviceClient;

        public DownloaderServicesController(IAdaptiveClient<IDownloaderServices> serviceClient)
        {
            this.serviceClient = serviceClient;
        }

        [HttpGet("GetAPIEndpoint")]
        public EndPointConfiguration GetAPIEndpoint()
        {
            List<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json")).ToList();
            IEndPointConfiguration ep = endPoints.FirstOrDefault(x => x.API_Name == API_Name.FREDLocal && x.EndPointType == EndPointType.HTTP);
            return ep as EndPointConfiguration;
        }


        [HttpGet("GetAllLocalObservations/{symbol}")]
        public async Task<JsonResult> GetAllLocalObservations(string symbol)
        {
            var data = await serviceClient.CallAsync(x => x.ObservationsService.GetLocalObservations(symbol));
            return new JsonResult(data);
        }

        [HttpGet("GetLocalObservations/{symbol}/{skip}/{take}")]
        public async Task<JsonResult> GetLocalObservations(string symbol, int skip, int take)
        {
            var data = await serviceClient.CallAsync(x => x.ObservationsService.GetLocalObservations(symbol, skip, take));
            return new JsonResult(data);
        }


        [HttpGet("UpdateLocalObservations/{symbol}")]
        public async Task<RowOpResult> UpdateLocalObservations(string symbol)
        {
            return await serviceClient.CallAsync(x => x.ObservationsService.UpdateLocalObservations(symbol));
        }

        [HttpGet("DeleteLocalObservations/{symbol}")]
        public async Task<RowOpResult> DeleteLocalObservations(string symbol)
        {
            return await serviceClient.CallAsync(x => x.ObservationsService.DeleteLocalObservations(symbol));
        }


        [HttpGet("GetLocalSeries/{skip}/{take}/{searchTitle?}")]
        public async Task<JsonResult> GetLocalSeries(int skip, int take, string searchTitle)
        {
            var data = await serviceClient.CallAsync(x => x.SeriesService.GetLocalSeries(skip, take, searchTitle));
            return new JsonResult(data);
        }


        [HttpGet("SaveLocalSeries/{symbol}")]
        public async Task<RowOpResult> SaveLocalSeries(string symbol)
        {
            return await serviceClient.CallAsync(x => x.SeriesService.SaveLocalSeries(symbol));
        }

        [HttpGet("DeleteLocalSeries/{symbol}")]
        public async Task<RowOpResult> DeleteLocalSeries(string symbol)
        {
            return await serviceClient.CallAsync(x => x.SeriesService.DeleteLocalSeries(symbol));
        }

        [HttpGet("GetLocalSeriesSymbols")]
        public async Task<IEnumerable<string>> GetLocalSeriesSymbols()
        {
            return await serviceClient.CallAsync(x => x.SeriesService.GetLocalSeriesSymbols());
        }
    }
}
