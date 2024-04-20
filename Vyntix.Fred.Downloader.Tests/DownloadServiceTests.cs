using LeaderAnalytics.Vyntix.Fred.Domain.Downloader;
using LeaderAnalytics.Vyntix.Fred.Downloader.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vyntix.Fred.Downloader.Tests;
public class DownloadServiceTests : BaseTest
{
    public DownloadServiceTests(string currentProviderName) : base(currentProviderName)
    {
        
    }


    [Test]
    public async Task Series_path_download_test_for_GNPCA()
    {
        FredDownloadArgs args = new FredDownloadArgs
        {
            Symbols = ["GNPCA"],
            Series = true,
            IncludeDiscontinuedSeries = true,
            SeriesCategories = true,
            ChildCategories = true,
            SeriesTags = true,
            CategoryTags = true,
            RelatedCategories = true,
            Releases = true,
            ReleaseDates = true,
            Sources = true,
            Observations = true,
            Recurse = true
        };
        await client.CallAsync(x => x.DownloadService.Download(args, null));
    }


    [Test]
    public async Task Catagory_path_download_test_for_160()
    {
        FredDownloadArgs args = new FredDownloadArgs
        {
            CategoryID = "106" ,
            Series = true,
            IncludeDiscontinuedSeries = true,
            SeriesCategories = true,
            ChildCategories = true,
            SeriesTags = true,
            CategoryTags = true,
            RelatedCategories = true,
            Releases = true,
            ReleaseDates = true,
            Sources = true,
            Observations = true,
            Recurse = true
        };
        await client.CallAsync(x => x.DownloadService.Download(args, null));
    }

}
