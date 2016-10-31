using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DiagnosticAdapter;

namespace ApplicationInsights.Listener
{
    internal class EntityFrameworkListener
    {
        private TelemetryClient _client;
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        private readonly AsyncLocal<long> _beginDependencyTimestamp = new AsyncLocal<long>();

        public EntityFrameworkListener(TelemetryClient client)
        {
            _client = client;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.BeforeExecuteCommand")]
        public void OnBeginCommand(DbCommand Command, string ExecuteMethod, Guid InstanceId, long Timestamp, bool IsAsync)
        {
            _beginDependencyTimestamp.Value = Stopwatch.GetTimestamp();
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.AfterExecuteCommand")]
        public void OnEndCommand(DbCommand Command, string ExecuteMethod, Guid InstanceId, long Timestamp, bool IsAsync)
        {
            var start = _beginDependencyTimestamp.Value;
            var end = Stopwatch.GetTimestamp();

            var telemetry = new DependencyTelemetry();
            telemetry.Name = "Microsoft.EntityFrameworkCore.ExecuteCommand";
            telemetry.CommandName = ExecuteMethod;
            telemetry.Duration = TimeSpan.FromTicks((long)((end - start) * TimestampToTicks));
            telemetry.StartTime = DateTime.Now - telemetry.Duration;
            _client.TrackDependency(telemetry);
        }
    }
}
