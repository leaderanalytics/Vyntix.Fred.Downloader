using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader.Blazor.Shared.FREDLocal
{
    public interface IDownloaderServices
    {
        IObservationsService ObservationsService { get; }
        ISeriesService SeriesService { get;  }
    }
}
