using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using LeaderAnalytics.AdaptiveClient;
using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using System.IO;

namespace Downloader.Blazor.Client
{
    public class Program
    {
        private static EndPointConfiguration endPoint;

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            HttpClient httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            Console.WriteLine("base address is " + builder.HostEnvironment.BaseAddress);
            endPoint = await httpClient.GetFromJsonAsync<EndPointConfiguration>("DownloaderServices/GetAPIEndpoint");

            if (endPoint == null)
                throw new Exception("No endpoints were found.");

            builder.RootComponents.Add<App>("#app");
            builder.ConfigureContainer(new AutofacServiceProviderFactory(containerBuilder => {
                RegistrationHelper registrationHelper = new RegistrationHelper(containerBuilder);
                registrationHelper.RegisterModule(new Downloader.APIClient.AdaptiveClientModule(endPoint));
            }));
            builder.Services.AddScoped(sp => httpClient);  // used internally by Blazor.
            builder.Services.AddHttpClient("ServerAPI", client => {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
                });  // Used by this app

            builder.Services.AddSingleton<MessageService>();
            builder.Services.AddMudServices();
            await builder.Build().RunAsync();
        }
    }
}
