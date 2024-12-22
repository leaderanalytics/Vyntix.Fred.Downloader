namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public static class ServiceCollectionExtensions
{
    public static RegistrationValues AddFredDownloaderServices(this RegistrationHelper registrationHelper) => new RegistrationValues(registrationHelper);
}


public class RegistrationValues
{
    public RegistrationValues(RegistrationHelper registrationHelper)
    { 
        new AdaptiveClientModule().Register(registrationHelper);
        registrationHelper.Builder.RegisterModule(new AutofacModule());
    }
}
