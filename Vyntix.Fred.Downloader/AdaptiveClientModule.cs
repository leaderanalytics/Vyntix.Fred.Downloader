namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class AdaptiveClientModule : IAdaptiveClientModule
{
    private readonly IEnumerable<IEndPointConfiguration> endPoints;

    public AdaptiveClientModule(IEnumerable<IEndPointConfiguration> endPoints) => this.endPoints = endPoints ?? throw new ArgumentNullException(nameof(endPoints));

    public void Register(RegistrationHelper registrationHelper)
    {
        ArgumentNullException.ThrowIfNull(registrationHelper);
        ArgumentNullException.ThrowIfNull(endPoints);

        registrationHelper

            // Always register endPoints first
            .RegisterEndPoints(endPoints)

            // EndPoint Validator
            .RegisterEndPointValidator<MSSQL_EndPointValidator>(EndPointType.InProcess, DatabaseProviderName.MSSQL)
            .RegisterEndPointValidator<MySQL_EndPointValidator>(EndPointType.InProcess, DatabaseProviderName.MySQL)

            // DbContextOptions
            .RegisterDbContextOptions<DbContextOptions_MSSQL>(DatabaseProviderName.MSSQL)
            .RegisterDbContextOptions<DbContextOptions_MySQL>(DatabaseProviderName.MySQL)

            // DbContext
            .RegisterDbContext<FREDStagingDb>(API_Name.FRED_Staging)

            // Migration Contexts
            .RegisterMigrationContext<FREDStagingDbMSSQL>(API_Name.FRED_Staging, DatabaseProviderName.MSSQL)
            .RegisterMigrationContext<FREDStagingDbMySQL>(API_Name.FRED_Staging, DatabaseProviderName.MySQL)

            // Database Initializers
            .RegisterDatabaseInitializer<DatabaseInitializer>(API_Name.FRED_Staging, DatabaseProviderName.MSSQL)
            .RegisterDatabaseInitializer<DatabaseInitializer>(API_Name.FRED_Staging, DatabaseProviderName.MySQL)

            // Service Manifests
            .RegisterServiceManifest<API_Manifest, IAPI_Manifest>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MSSQL)
            .RegisterServiceManifest<API_Manifest, IAPI_Manifest>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MySQL)

            // Services - MSSQL
            .RegisterService<CategoriesService, ICategoriesService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MSSQL)
            .RegisterService<ObservationsService, IObservationsService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MSSQL)
            .RegisterService<ReleasesService, IReleasesService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MSSQL)
            .RegisterService<SeriesService, ISeriesService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MSSQL)
            .RegisterService<DownloadService, IDownloadService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MSSQL)
            .RegisterService<AuthenticationService, IAuthenticationService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MSSQL)


            // Services - MySQL
            .RegisterService<CategoriesService, ICategoriesService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MySQL)
            .RegisterService<ObservationsService, IObservationsService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MySQL)
            .RegisterService<ReleasesService, IReleasesService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MySQL)
            .RegisterService<SeriesService, ISeriesService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MySQL)
            .RegisterService<DownloadService, IDownloadService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MySQL)
            .RegisterService<AuthenticationService, IAuthenticationService>(EndPointType.InProcess, API_Name.FRED_Staging, DatabaseProviderName.MySQL);
    }
}
