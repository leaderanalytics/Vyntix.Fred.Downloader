using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Autofac.Extensions.DependencyInjection;
using Downloader.Blazor.Server;
using LeaderAnalytics.AdaptiveClient;
using Downloader.Blazor.Shared;
using System.IO;
using System.Reflection;

namespace Downloader.Tests
{
    // https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0

    public class WebApp<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public List<IEndPointConfiguration> endPoints;
        public IHost WebHost;
        

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls("https://localhost:5001");  // does not work
            
            base.ConfigureWebHost(builder);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            endPoints = EndPointUtilities.LoadEndPoints(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json")).ToList();
            EndPointConfiguration endPoint = endPoints.Cast<EndPointConfiguration>().FirstOrDefault(x => x.API_Name == API_Name.FREDLocal && x.EndPointType == EndPointType.HTTP);

            builder
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(containerBuilder =>
                {
                    RegistrationHelper registrationHelper = new RegistrationHelper(containerBuilder);
                    registrationHelper.RegisterModule(new Downloader.APIClient.AdaptiveClientModule(endPoint));
                }));


            

            WebHost = builder.Build();
            WebHost.Start();
            return WebHost; //base.CreateHost(builder);
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return base.CreateHostBuilder();
        }
    }
}
