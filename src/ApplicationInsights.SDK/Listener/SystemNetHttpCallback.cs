using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DiagnosticAdapter;

namespace ApplicationInsights.Listener
{
    public class SystemNetHttpCallback
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private readonly TelemetryClient _client;
        private readonly AsyncLocal<long> _beginDependencyTimestamp = new AsyncLocal<long>();

        public SystemNetHttpCallback(TelemetryClient client)
        {
            _client = client;
        }

        [DiagnosticName("System.Net.Http.Request")]
        public void OnOutgoingHttpRequest(HttpRequestMessage Request, Guid LoggingRequestId)
        {
            _beginDependencyTimestamp.Value = Stopwatch.GetTimestamp();
        }

        [DiagnosticName("System.Net.Http.Response")]
        public void OnOutgoingHttpResponse(HttpResponseMessage Response, Guid LoggingRequestId)
        {
            var start = _beginDependencyTimestamp.Value;
            var end = Stopwatch.GetTimestamp();

            var telemetry = new DependencyTelemetry();
            telemetry.Name = "System.Net.Http";
            telemetry.DependencyTypeName = "System.Net.Http";
            telemetry.Duration = TimeSpan.FromTicks((long)((end - start) * TimestampToTicks));
            telemetry.Success = Response.IsSuccessStatusCode;

            _client.TrackDependency(telemetry);
        }
    }
}