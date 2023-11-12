using LeaderAnalytics.Vyntix.Fred.Domain;
using LeaderAnalytics.Vyntix.Fred.Domain.Downloader;
using LeaderAnalytics.Vyntix.Fred.StagingDb;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;

namespace LeaderAnalytics.Vyntix.Fred.Downloader.Tests;

[TestFixture("MSSQL")]
[TestFixture("MySQL")]
public abstract class BaseTest
{
    protected ILifetimeScope scope;
    protected IAdaptiveClient<IAPI_Manifest> client;
    protected readonly List<IEndPointConfiguration> endPoints;
    protected FREDStagingDb db;
    private IHost host;
    protected string CurrentProviderName { get; set; }
    protected IEndPointConfiguration EndPoint { get; set; }
    protected IFredClient fredClient;
    protected IServiceCollection services {  get; set; }

    public BaseTest(string currentProviderName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, buffered: false)
            .CreateLogger();

        Log.Information("Logger created.");
        CurrentProviderName = currentProviderName;  // Passed in by NUnit from the TestFixture attribute
        // Load all endpoints and make them active
        endPoints = EndPointUtilities.LoadEndPoints("appsettings.development.json", false).ToList();
        endPoints.ForEach(x => x.IsActive = true);  // AdaptiveClient only looks at active endpoints
    }

    [SetUp]
    [OneTimeSetUp]
    protected virtual async Task Setup()
    {
        host = Host.CreateDefaultBuilder()
        .ConfigureHostConfiguration(builder => builder.AddJsonFile("appsettings.development.json", false))
        .ConfigureServices((config, services) => {
            string apiKey = config.Configuration.GetValue<string>("FredAPI_Key");
            services.AddFredClient()
            .UseAPIKey(apiKey)
            .UseConfig(x => new FredClientConfig { MaxDownloadRetries = 3 });

            // Setting MaxDownloadRetries to 1 will cause FredClient to abort on the first 429 error

        })
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>((config, containerBuilder) =>
        {
            containerBuilder.AddFredDownloaderServices(endPoints);
        }).Build();

        scope = host.Services.GetAutofacRoot().BeginLifetimeScope();
        client = scope.Resolve<IAdaptiveClient<IAPI_Manifest>>();
        fredClient = scope.Resolve<IFredClient>();
        ResolutionHelper resolutionHelper = scope.Resolve<ResolutionHelper>();
        EndPoint = endPoints.First(x => x.API_Name == API_Name.FRED_Staging && x.ProviderName == CurrentProviderName);
        EndPoint.Preference = 1;
        endPoints.Where(x => x.Name != EndPoint.Name).ToList().ForEach(x => x.Preference = 2);
        db = resolutionHelper.ResolveDbContext(EndPoint) as FREDStagingDb;
        IDatabaseUtilities databaseUtilities = scope.Resolve<IDatabaseUtilities>();
        await databaseUtilities.DropDatabase(EndPoint);
        DatabaseValidationResult result = await databaseUtilities.CreateOrUpdateDatabase(EndPoint);

        if (!result.DatabaseWasCreated)
            throw new Exception("Database was not created.");
    }

    [TearDown]
    [OneTimeTearDown]
    protected virtual async Task TearDown()
    { 
        scope.Dispose();
        host.Dispose();
    }
}
