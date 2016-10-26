using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using ApplicationInsights.Logging;

namespace ApplicationInsights.Extensions
{
    public static class LoggerFactoryApplicationInsightsExtensions
    {
        public static ILoggerFactory AddApplicationInsights(this ILoggerFactory factory, TelemetryClient client)
        {
            factory.AddProvider(new ApplicationsInsightsLoggerProvider(client));
            return factory;
        }
    }
}
