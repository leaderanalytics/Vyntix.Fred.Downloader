﻿namespace Downloader.Blazor.Shared.FREDLocal;

public interface ISeriesService
{
    Task<IEnumerable<Series>> GetLocalSeries(int skip, int take, string searchTitle);
    Task<IEnumerable<string>> GetLocalSeriesSymbols();
    Task<RowOpResult> SaveLocalSeries(string symbol);
    Task<RowOpResult> DeleteLocalSeries(string symbol);
}
