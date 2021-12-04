namespace Downloader.Tests;

// https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0

public class WebApp<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public List<IEndPointConfiguration> endPoints;
    public IHost WebHost;


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("https://localhost:5001");  // does not work

        base.ConfigureWebHost(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        endPoints = EndPointUtilities.LoadEndPoints(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json")).ToList();
        EndPointConfiguration endPoint = endPoints.Cast<EndPointConfiguration>().FirstOrDefault(x => x.API_Name == API_Name.FREDLocal && x.EndPointType == EndPointType.HTTP);

        builder
            .UseServiceProviderFactory(new AutofacServiceProviderFactory(containerBuilder =>
            {
                RegistrationHelper registrationHelper = new RegistrationHelper(containerBuilder);
                registrationHelper.RegisterModule(new Downloader.APIClient.AdaptiveClientModule(endPoint));
            }));




        WebHost = builder.Build();
        WebHost.Start();
        return WebHost; //base.CreateHost(builder);
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base.CreateHostBuilder();
    }
}
