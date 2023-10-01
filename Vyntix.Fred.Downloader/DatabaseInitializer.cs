namespace LeaderAnalytics.Vyntix.Fred.Downloader;

public class DatabaseInitializer : IDatabaseInitializer
{
    private FREDStagingDb db;


    public DatabaseInitializer(FREDStagingDb db)
    {
        this.db = db;
    }

    public async Task Seed(string migrationName)
    {

    }
}
