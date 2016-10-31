using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DiagnosticAdapter;

namespace ApplicationInsights.Listener
{
    internal class SqlClientListener
    {
        private TelemetryClient _client;
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        private readonly AsyncLocal<long> _beginDependencyTimestamp = new AsyncLocal<long>();

        public SqlClientListener(TelemetryClient client)
        {
            _client = client;
        }

        //[DiagnosticName("System.Data.SqlClient.WriteCommandBefore")]
        public void OnBeginCommand(/*Guid OperationId, string Operation, string ConnectionId, SqlCommand Command*/)
        {
            
            //_beginDependencyTimestamp.Value = Stopwatch.GetTimestamp();
        }

        [DiagnosticName("System.Data.SqlClient.WriteCommandAfter")]
        public void OnEndCommand(/*Guid OperationId, string Operation, string ConnectionId, SqlCommand Command*/)
        {
            //var start = _beginDependencyTimestamp.Value;
            //var end = Stopwatch.GetTimestamp();

            //var telemetry = new DependencyTelemetry();
            //telemetry.Name = "Microsoft.EntityFrameworkCore.ExecuteCommand";
            //telemetry.Duration = TimeSpan.FromTicks((long)((end - start) * TimestampToTicks));
            //telemetry.StartTime = DateTime.Now - telemetry.Duration;
            //_client.TrackDependency(telemetry);
        }
    }
}
