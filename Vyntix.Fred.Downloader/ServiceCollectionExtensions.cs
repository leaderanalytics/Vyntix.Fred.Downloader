using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public static class ServiceCollectionExtensions
{
    public static RegistrationValues AddFredDownloaderServices(this ContainerBuilder containerBuilder, IEnumerable<IEndPointConfiguration> endpoints) => new RegistrationValues(containerBuilder, endpoints);
}


public class RegistrationValues
{
    private readonly ContainerBuilder containerBuilder;
    private IEnumerable<IEndPointConfiguration> endpoints;

    public RegistrationValues(ContainerBuilder containerBuilder, IEnumerable<IEndPointConfiguration> endpoints)
    { 
        this.containerBuilder = containerBuilder;
        this.endpoints = endpoints;
        Build();
    }

    private void Build() 
    {
        RegistrationHelper registrationHelper = new RegistrationHelper(containerBuilder);
        new AdaptiveClientModule(endpoints).Register(registrationHelper);
        containerBuilder.RegisterModule(new AutofacModule());
    }
}
