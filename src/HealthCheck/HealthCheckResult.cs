namespace HealthChecks
{
    public class HealthCheckResult
    {
        public CheckStatus CheckStatus { get; }
        public string Description { get; }

        private HealthCheckResult(CheckStatus checkStatus, string description)
        {
            CheckStatus = checkStatus;
            Description = description;
        }

        public static HealthCheckResult Unhealthy(string description)
        {
            return new HealthCheckResult(CheckStatus.Unhealthy, description);
        }

        public static HealthCheckResult Healthy(string description)
        {
            return new HealthCheckResult(CheckStatus.Healthy, description);
        }

        public static HealthCheckResult Warning(string description)
        {
            return new HealthCheckResult(CheckStatus.Warning, description);
        }

        public static HealthCheckResult Unknown(string description)
        {
            return new HealthCheckResult(CheckStatus.Unknown, description);
        }
    }
}
