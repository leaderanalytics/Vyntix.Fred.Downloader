using Autofac;
using Downloader.Blazor.Shared;
using LeaderAnalytics.AdaptiveClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Downloader.Services
{
    public class AutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Autofac & AdaptiveClient

            // Don't forget to do this:
            builder.RegisterModule(new LeaderAnalytics.AdaptiveClient.EntityFrameworkCore.AutofacModule());

            RegistrationHelper registrationHelper = new RegistrationHelper(builder);
            registrationHelper.RegisterModule(new AdaptiveClientModule());
        }
    }
}
