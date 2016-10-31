using Microsoft.ApplicationInsights;
using ApplicationInsights;
using ApplicationInsights.Extensions;
using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderApplicationInsightsExtensions
    {
        public static IWebHostBuilder UseApplicationInsights(this IWebHostBuilder hostBuilder)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            var client = new TelemetryClient();
            var observer = new ApplicationInsightsObserver(client);
            DiagnosticListener.AllListeners.Subscribe(observer);

            return hostBuilder.ConfigureLogging(loggerFactory =>
            {
                loggerFactory.AddApplicationInsights(client);
            });
        }
    }
}
