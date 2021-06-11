using System;
using System.Collections.Generic;
using System.Linq;
using Downloader.Blazor.Shared;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.Utilities;
using Downloader.Blazor.Shared.FREDLocal;
using System.IO;
using System.Reflection;
using Downloader.Shared;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Downloader.APIClient
{
    public class AdaptiveClientModule :IAdaptiveClientModule
    {
        private EndPointConfiguration endPoint;
        public AdaptiveClientModule(EndPointConfiguration endPoint) => this.endPoint = endPoint;
        

        public void Register(RegistrationHelper registrationHelper)
        {

            registrationHelper
                
                // Always register endPoints first
                .RegisterEndPoints(new List<IEndPointConfiguration> { endPoint })

                // EndPoint Validator
                .RegisterEndPointValidator<Downloader.APIClient.HTTP_EndPointValidator>(EndPointType.HTTP, DatabaseProviderName.NONE)

                // WebAPI 
                .RegisterService<ObservationsService, IObservationsService>(EndPointType.HTTP, API_Name.FREDLocal, DatabaseProviderName.NONE)
                .RegisterService<SeriesService, ISeriesService>(EndPointType.HTTP, API_Name.FREDLocal, DatabaseProviderName.NONE)

                // Service Manifests
                .RegisterServiceManifest<ServiceManifest, IDownloaderServices>(EndPointType.HTTP, API_Name.FREDLocal, DatabaseProviderName.NONE);
        }
    }
}
