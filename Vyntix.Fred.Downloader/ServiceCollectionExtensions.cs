using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public static class ServiceCollectionExtensions
{
    public static RegistrationValues AddFredDownloaderServices(this ContainerBuilder containerBuilder) => new RegistrationValues(containerBuilder);
}


public class RegistrationValues
{
    private readonly ContainerBuilder containerBuilder;

    public RegistrationValues(ContainerBuilder containerBuilder)
    { 
        this.containerBuilder = containerBuilder;
        Build();
    }

    private void Build() 
    {
        RegistrationHelper registrationHelper = new RegistrationHelper(containerBuilder);
        new AdaptiveClientModule().Register(registrationHelper);
        containerBuilder.RegisterModule(new AutofacModule());
    }
}
