using Microsoft.ApplicationInsights;
using ApplicationInsights.Extensions;
using ApplicationInsights.Listener;
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
            var listener = new ApplicationInsightsListener(client);
            DiagnosticListener.AllListeners.Subscribe(listener);

            return hostBuilder.ConfigureLogging(loggerFactory =>
            {
                loggerFactory.AddApplicationInsights(client);
            });
        }
    }
}
