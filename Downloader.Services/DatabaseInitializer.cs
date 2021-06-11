using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader.Services
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private Db db;


        public DatabaseInitializer(Db db)
        {
            this.db = db;
        }

        public async Task Seed(string migrationName)
        {
            
        }
    }
}
