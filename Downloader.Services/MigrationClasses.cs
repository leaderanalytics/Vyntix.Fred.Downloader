namespace Downloader.Services;

public class MigrationConstants
{
    public static readonly string EndPointsFile;
    static MigrationConstants() => EndPointsFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json");
}

public class MSSQLContextFactory : IDesignTimeDbContextFactory<Db_MSSQL>
{
    public Db_MSSQL CreateDbContext(string[] args)
    {
        string connectionString = ConnectionstringUtility.GetConnectionString(MigrationConstants.EndPointsFile, API_Name.FREDLocal, DatabaseProviderName.MSSQL);
        DbContextOptionsBuilder dbOptions = new DbContextOptionsBuilder();
        dbOptions.UseSqlServer(connectionString);
        Db_MSSQL db = new Db_MSSQL(dbOptions.Options);
        return db;
    }
}

public class MySQLContextFactory : IDesignTimeDbContextFactory<Db_MySQL>
{
    public Db_MySQL CreateDbContext(string[] args)
    {
        string connectionString = ConnectionstringUtility.BuildConnectionString(ConnectionstringUtility.GetConnectionString(MigrationConstants.EndPointsFile, API_Name.FREDLocal, DatabaseProviderName.MySQL));
        DbContextOptionsBuilder dbOptions = new DbContextOptionsBuilder();
        dbOptions.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        Db_MySQL db = new Db_MySQL(dbOptions.Options);
        return db;
    }
}
