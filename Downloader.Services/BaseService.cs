using Downloader.Blazor.Shared.FREDLocal;
using LeaderAnalytics.Vyntix.Fred.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader.Services
{
    public abstract class BaseService
    {
        protected readonly Db db;
        protected readonly IFredClient fredClient;
        protected readonly IDownloaderServices serviceManifest;
        
        public BaseService(Db db, IDownloaderServices serviceManifest, IFredClient fredClient)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.fredClient = fredClient ?? throw new ArgumentNullException(nameof(fredClient));
            this.serviceManifest = serviceManifest ?? throw new ArgumentNullException(nameof(serviceManifest));
        }
    }
}
