using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HealthChecks
{
    public class HealthCheckService : IHealthCheckService
    {
        public Dictionary<string, Func<ValueTask<bool>>> _checks;

        private ILogger<HealthCheckService> _logger;

        public HealthCheckResults CheckResults { get; set; }

        public HealthCheckService(HealthCheckBuilder builder, ILogger<HealthCheckService> logger)
        {
            _checks = builder.Checks;
            _logger = logger;
        }

        public async Task<bool> CheckHealthAsync()
        {
            StringBuilder logMessage = new StringBuilder();
            CheckResults = new HealthCheckResults();
            CheckResults.CheckResults = new Dictionary<string, HealthCheckResult>();

            var healthy = true;
            foreach(var check in _checks)
            {
                try
                {
                    healthy &= await check.Value();
                    logMessage.AppendLine($"HealthCheck: {check.Key} : {(healthy ? "Healthy" : "Unhealthy")}");
                }
                catch
                {
                    healthy &= false;
                }
            }

            _logger.Log((healthy ? LogLevel.Information : LogLevel.Error), 0, logMessage, null, MessageFormatter);            
            return healthy;
        }

        private static string MessageFormatter(object state, Exception error)
        {
            return state.ToString();
        }
    } 
}
