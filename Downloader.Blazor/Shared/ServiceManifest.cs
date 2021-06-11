using Downloader.Blazor.Shared.FREDLocal;
using LeaderAnalytics.AdaptiveClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader.Shared
{
    public class ServiceManifest : ServiceManifestFactory, IDownloaderServices
    {
        public IObservationsService ObservationsService => Create<IObservationsService>();
        public ISeriesService SeriesService => Create<ISeriesService>();
    }
}
