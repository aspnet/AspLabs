using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HealthChecks
{
    public class HealthCheckService : IHealthCheckService
    {
        public Dictionary<string, Func<ValueTask<IHealthCheckResult>>> _checks;

        private ILogger<IHealthCheckService> _logger;
       

        public HealthCheckService(HealthCheckBuilder builder, ILogger<IHealthCheckService> logger)
        {
            _checks = builder.Checks;
            _logger = logger;
        }

        public async Task<HealthCheckResults> CheckHealthAsync()
        {
            StringBuilder logMessage = new StringBuilder();
            var checkResults = new HealthCheckResults();

            var healthy = true;
            foreach (var check in _checks)
            {
                try
                {
                    var healthCheckResult = await check.Value();
                    checkResults.CheckResults.Add(healthCheckResult);
                    healthy &= healthCheckResult.CheckStatus == CheckStatus.Healthy || healthCheckResult.CheckStatus == CheckStatus.Warning;
                    logMessage.AppendLine($"HealthCheck: {check.Key} : {(healthy ? "Healthy" : "Unhealthy")}");
                }
                catch
                {
                    healthy &= false;
                }
            }

            _logger.Log((healthy ? LogLevel.Information : LogLevel.Error), 0, logMessage, null, MessageFormatter);
            return checkResults;
        }

        private static string MessageFormatter(object state, Exception error)
        {
            return state.ToString();
        }
    }
}
