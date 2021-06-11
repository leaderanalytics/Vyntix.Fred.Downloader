using Downloader.Blazor.Shared.FREDLocal;
using LeaderAnalytics.AdaptiveClient;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Collections.Generic;
using LeaderAnalytics.Vyntix.Fred.Model;
using System;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using System.IO;
using System.Reflection;
using System.Linq;
using Downloader.Blazor.Shared;
using System.Net.Http;
using System.Net.Http.Json;
using Downloader.Blazor.Server;

namespace Downloader.Tests
{
    public class Tests : BaseTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            IAdaptiveClient<IDownloaderServices> serviceClient = services.GetService<IAdaptiveClient<IDownloaderServices>>();
            IEnumerable<Observation> observations =  await serviceClient.CallAsync(x => x.ObservationsService.GetLocalObservations("NROU"));
            Assert.Pass();
        }


        [Test]
        public async Task Test2()
        {
            // https://andrewlock.net/converting-integration-tests-to-net-core-3/
            // webapp = new WebApp<Startup>();
            HttpClient httpClient = this.webapp.CreateClient();
            string url = httpClient.BaseAddress.ToString(); // http://localhost/

            var junk = await httpClient.GetAsync(httpClient.BaseAddress);
            


            IAdaptiveClient<IDownloaderServices> serviceClient = services.GetService<IAdaptiveClient<IDownloaderServices>>();
            EndPointConfiguration endPoint = endPoints.Cast<EndPointConfiguration>().FirstOrDefault(x => x.API_Name == API_Name.FREDLocal && x.EndPointType == EndPointType.HTTP);
            IEnumerable<Observation> observations = await serviceClient.CallAsync(x => x.ObservationsService.GetLocalObservations("NROU"), endPoint.Name);
            Assert.Pass();
        }
    }
}