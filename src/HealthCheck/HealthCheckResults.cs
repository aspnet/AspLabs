using System.Collections.Generic;

namespace HealthChecks
{
    public class HealthCheckResults
    {
        public Dictionary<string, HealthCheckResult> CheckResults { get; set; }
    }

    public class HealthCheckResult
    {
        public bool Success { get; set; }
        public CheckStatus CheckStatus { get; set; }

        public string Description { get; set; }

        public string AdditionalData { get; set; }

        public string CheckType { get; set; }
    }

    public enum CheckStatus
    {
        Unknown,
        Failed,
        Ok,
        Warning
    }

}
