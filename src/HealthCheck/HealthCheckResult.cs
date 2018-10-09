using System.Collections;
using System.Collections.Generic;

namespace HealthChecks
{
    public class HealthCheckResult
    {
        public CheckStatus CheckStatus { get; }
        public string Description { get; }
        public ICollection<HealthCheckResult> InnerResults { get; private set; } 

        private HealthCheckResult(CheckStatus checkStatus, string description, ICollection<HealthCheckResult> innerResults)
        {
            CheckStatus = checkStatus;
            Description = description;
            InnerResults = innerResults ?? new List<HealthCheckResult>();
        }

        public static HealthCheckResult Unhealthy(string description)
        {
            return new HealthCheckResult(CheckStatus.Unhealthy, description, null);
        }
        public static HealthCheckResult Unhealthy(string description, ICollection<HealthCheckResult> innerResults)
        {
            return new HealthCheckResult(CheckStatus.Unhealthy, description, innerResults);
        }

        public static HealthCheckResult Healthy(string description)
        {
            return new HealthCheckResult(CheckStatus.Healthy, description, null);
        }
        public static HealthCheckResult Healthy(string description, ICollection<HealthCheckResult> innerResults)
        {
            return new HealthCheckResult(CheckStatus.Healthy, description, innerResults);
        }

        public static HealthCheckResult Warning(string description)
        {
            return new HealthCheckResult(CheckStatus.Warning, description, null);
        }
        public static HealthCheckResult Warning(string description, ICollection<HealthCheckResult> innerResults)
        {
            return new HealthCheckResult(CheckStatus.Warning, description, innerResults);
        }

        public static HealthCheckResult Unknown(string description)
        {
            return new HealthCheckResult(CheckStatus.Unknown, description, null);
        }
        public static HealthCheckResult Unknown(string description, ICollection<HealthCheckResult> innerResults)
        {
            return new HealthCheckResult(CheckStatus.Unknown, description, innerResults);
        }
    }
}
