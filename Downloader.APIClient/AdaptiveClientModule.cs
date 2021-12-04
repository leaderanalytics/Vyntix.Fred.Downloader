namespace Downloader.APIClient;

public class AdaptiveClientModule : IAdaptiveClientModule
{
    private EndPointConfiguration endPoint;
    public AdaptiveClientModule(EndPointConfiguration endPoint) => this.endPoint = endPoint;


    public void Register(RegistrationHelper registrationHelper)
    {

        registrationHelper

            // Always register endPoints first
            .RegisterEndPoints(new List<IEndPointConfiguration> { endPoint })

            // EndPoint Validator
            .RegisterEndPointValidator<HTTP_EndPointValidator>(EndPointType.HTTP, DatabaseProviderName.NONE)

            // WebAPI 
            .RegisterService<ObservationsService, IObservationsService>(EndPointType.HTTP, API_Name.FREDLocal, DatabaseProviderName.NONE)
            .RegisterService<SeriesService, ISeriesService>(EndPointType.HTTP, API_Name.FREDLocal, DatabaseProviderName.NONE)

            // Service Manifests
            .RegisterServiceManifest<ServiceManifest, IDownloaderServices>(EndPointType.HTTP, API_Name.FREDLocal, DatabaseProviderName.NONE);
    }
}
