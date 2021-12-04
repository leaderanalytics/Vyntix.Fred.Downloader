namespace Downloader.Services;

public class AdaptiveClientModule : IAdaptiveClientModule
{
    public void Register(RegistrationHelper registrationHelper)
    {
        List<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json")).ToList();
        IEndPointConfiguration mySQLconn = endPoints.FirstOrDefault(x => x.API_Name == API_Name.FREDLocal && x.ProviderName == DatabaseProviderName.MySQL);

        if (mySQLconn != null)
            mySQLconn.ConnectionString = ConnectionstringUtility.BuildConnectionString(mySQLconn.ConnectionString);


        registrationHelper

            // Always register endPoints first
            .RegisterEndPoints(endPoints)

            // EndPoint Validator
            .RegisterEndPointValidator<MSSQL_EndPointValidator>(EndPointType.DBMS, DatabaseProviderName.MSSQL)
            .RegisterEndPointValidator<MySQL_EndPointValidator>(EndPointType.DBMS, DatabaseProviderName.MySQL)

            // DbContextOptions
            .RegisterDbContextOptions<DbContextOptions_MSSQL>(DatabaseProviderName.MSSQL)
            .RegisterDbContextOptions<DbContextOptions_MySQL>(DatabaseProviderName.MySQL)

            // MSSQL
            .RegisterService<ObservationsService, IObservationsService>(EndPointType.DBMS, API_Name.FREDLocal, DatabaseProviderName.MSSQL)
            .RegisterService<SeriesService, ISeriesService>(EndPointType.DBMS, API_Name.FREDLocal, DatabaseProviderName.MSSQL)

            // MySQL
            .RegisterService<ObservationsService, IObservationsService>(EndPointType.DBMS, API_Name.FREDLocal, DatabaseProviderName.MySQL)
            .RegisterService<SeriesService, ISeriesService>(EndPointType.DBMS, API_Name.FREDLocal, DatabaseProviderName.MySQL)

            // DbContext
            .RegisterDbContext<Db>(API_Name.FREDLocal)

            // Migration Contexts
            .RegisterMigrationContext<Db_MSSQL>(API_Name.FREDLocal, DatabaseProviderName.MSSQL)
            .RegisterMigrationContext<Db_MySQL>(API_Name.FREDLocal, DatabaseProviderName.MySQL)

            // Database Initializers
            .RegisterDatabaseInitializer<DatabaseInitializer>(API_Name.FREDLocal, DatabaseProviderName.MSSQL)
            .RegisterDatabaseInitializer<DatabaseInitializer>(API_Name.FREDLocal, DatabaseProviderName.MySQL)

            // Service Manifests
            .RegisterServiceManifest<ServiceManifest, IDownloaderServices>(EndPointType.DBMS, API_Name.FREDLocal, DatabaseProviderName.MSSQL)
            .RegisterServiceManifest<ServiceManifest, IDownloaderServices>(EndPointType.DBMS, API_Name.FREDLocal, DatabaseProviderName.MySQL);
    }
}
