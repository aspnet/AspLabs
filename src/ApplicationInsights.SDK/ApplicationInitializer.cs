using System.Diagnostics;
using ApplicationInsights.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace ApplicationInsights.AspNetCore
{
    public class ApplicationInitializer
    {
        private readonly TelemetryClient _client;
        private readonly ILoggerFactory _loggerFactory;

        public ApplicationInitializer(
            ILoggerFactory factory,
            TelemetryClient client)
        {
            _loggerFactory = factory;
            _client = client;
        }

        internal void Initialize()
        {
            _loggerFactory.AddApplicationInsights(_client);
            var observer = new ApplicationInsightsObserver(_client);
            DiagnosticListener.AllListeners.Subscribe(observer);
        }
    }
}
