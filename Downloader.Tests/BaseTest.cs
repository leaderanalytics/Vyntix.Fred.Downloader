namespace Downloader.Tests;

public abstract class BaseTest
{
    protected readonly IServiceProvider services;
    protected readonly List<IEndPointConfiguration> endPoints;
    protected WebApp<Startup> webapp;

    public BaseTest()
    {

        //endPoints = EndPointUtilities.LoadEndPoints(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json")).ToList();
        //EndPointConfiguration endPoint = endPoints.Cast<EndPointConfiguration>().FirstOrDefault(x => x.API_Name == API_Name.FREDLocal && x.EndPointType == EndPointType.HTTP);

        //var host = Host.CreateDefaultBuilder()
        //        .UseServiceProviderFactory(new AutofacServiceProviderFactory(containerBuilder =>
        //        {
        //            RegistrationHelper registrationHelper = new RegistrationHelper(containerBuilder);
        //            registrationHelper.RegisterModule(new Downloader.APIClient.AdaptiveClientModule(endPoint));
        //        }))
        //        .ConfigureWebHostDefaults(builder =>
        //        {
        //            builder.UseStartup<Startup>();

        //        }).Build(); 

        //services = host.Services;
        webapp = new WebApp<Startup>();
        services = webapp.Services;
        endPoints = webapp.endPoints;

    }
}
