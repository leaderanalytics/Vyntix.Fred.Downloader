﻿namespace Downloader.Shared;

public static class ConnectionstringUtility
{
    public static string GetConnectionString(string filePath, string apiName, string providerName)
    {
        IEnumerable<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints(filePath, false);
        return endPoints.First(x => x.API_Name == apiName && x.ProviderName == providerName).ConnectionString;
    }

    public static string BuildConnectionString(string connectionString)
    {
        bool usePasswordFile = true; // change this value to true if you use a secrets file that is not checked into source control.

        if (usePasswordFile)
        {
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("C:\\Users\\sam\\AppData\\Roaming\\Blog\\appsettings.Development.json");  // path to your password file here
            IConfigurationRoot config = configBuilder.Build();
            connectionString = connectionString.Replace("{MySQL_UserName}", config["Data:MySQLUserName"]);
            connectionString = connectionString.Replace("{MySQL_Password}", config["Data:MySQLPassword"]);
        }

        return connectionString;
    }
}
