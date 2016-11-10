using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtension
    {
        public static IWebHostBuilder UseHealthChecks(this IWebHostBuilder builder, int port)
        {
            builder.ConfigureServices(services => {
                var existingUrl = builder.GetSetting(WebHostDefaults.ServerUrlsKey);
                builder.UseSetting(WebHostDefaults.ServerUrlsKey, $"{existingUrl};http://+:{port}");

                services.AddSingleton(new HealthCheckOptions{ HealthCheckPort = port});
                services.AddSingleton<IStartupFilter>(new HealthCheckStartupFilter(port));
            });
            return builder;
        }
    }
}
