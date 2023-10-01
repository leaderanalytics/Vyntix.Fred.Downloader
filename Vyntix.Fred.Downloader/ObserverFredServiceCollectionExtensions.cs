using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public static class ObserverFredServiceCollectionExtensions
{
    public static RegistrationValues AddObserverFredServices(this ContainerBuilder containerBuilder, IEnumerable<IEndPointConfiguration> endpoints) => new RegistrationValues(containerBuilder, endpoints);
}

public static class FredClientServiceCollectionExtensions
{
    public static LeaderAnalytics.Vyntix.Fred.FredClient.RegistrationValues AddFredClient(this IServiceCollection services)
    { 
        return LeaderAnalytics.Vyntix.Fred.FredClient.FredClientServiceCollectionExtensions.AddFredClient(services);
    }
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
