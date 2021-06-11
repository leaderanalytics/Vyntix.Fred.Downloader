using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Fred.Model;

namespace Downloader.Blazor.Shared.FREDLocal
{
    public interface IObservationsService
    {
        Task<List<Observation>> GetLocalObservations(string symbol);
        Task<List<Observation>> GetLocalObservations(string symbol, int skip, int take );
        Task<RowOpResult> UpdateLocalObservations(string symbols);
        Task<RowOpResult> DeleteLocalObservations(string symbol);
    }
}
