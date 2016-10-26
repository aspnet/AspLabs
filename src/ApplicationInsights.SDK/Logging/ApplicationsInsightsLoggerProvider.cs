using System.Collections.Concurrent;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace ApplicationInsights.Logging
{
    public class ApplicationsInsightsLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ApplicationInsightsLogger> _loggers = new ConcurrentDictionary<string, ApplicationInsightsLogger>();
        private TelemetryClient _telemetryClient;
        public ApplicationsInsightsLoggerProvider(TelemetryClient client)
        {
            _telemetryClient = client;
        }   
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        private ApplicationInsightsLogger CreateLoggerImplementation(string name)
        {
            return new ApplicationInsightsLogger(name, _telemetryClient);
        }

        public void Dispose()
        {
        }
    }
}
