using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using ApplicationInsights.Helpers;

namespace ApplicationInsights.Logging
{
    public class ApplicationInsightsLogger : ILogger
    {
        private string _categoryName;
        private string sdkVersion;
        private readonly TelemetryClient telemetryClient;

        public ApplicationInsightsLogger(string name, TelemetryClient telemetryClient)
        {
            _categoryName = name;
            this.telemetryClient = telemetryClient;
            this.sdkVersion = SdkVersionUtils.VersionPrefix + SdkVersionUtils.GetAssemblyVersion();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var structure = state as IEnumerable<KeyValuePair<string, object>>;

            var dict = new Dictionary<string, string>();
            dict["CategoryName"] = _categoryName;

            if (exception != null)
            {
                telemetryClient.TrackException(exception, dict);
            }
            else
            {
                var id = telemetryClient.Context.Operation.Id;
                telemetryClient.TrackTrace(formatter(state, exception), GetSeverityLevel(logLevel), dict);
            }
        }

        private SeverityLevel GetSeverityLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return SeverityLevel.Critical;
                case LogLevel.Error:
                    return SeverityLevel.Error;
                case LogLevel.Warning:
                    return SeverityLevel.Warning;
                case LogLevel.Information:
                case LogLevel.Debug:
                case LogLevel.Trace:
                default:
                    return SeverityLevel.Information;
            }
        }

    }
}
