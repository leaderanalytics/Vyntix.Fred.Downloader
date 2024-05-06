namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class AuthenticationService : BaseService, IAuthenticationService
{
    public AuthenticationService(FREDStagingDb db, IAPI_Manifest downloaderServices, IFredClient fredClient, ILogger<CategoriesService> logger, Action<string> statusCallback) : base(db, downloaderServices, fredClient, logger, statusCallback)
    {

    }

    public async Task<bool> IsAPI_KeyValid() => await fredClient.IsAPI_KeyValid();
    
}
