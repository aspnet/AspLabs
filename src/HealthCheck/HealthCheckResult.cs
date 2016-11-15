namespace HealthChecks
{
    public class HealthCheckResult
    {
        public CheckStatus CheckStatus { get; private set; }
        public string Description { get; private set; }

        private HealthCheckResult(CheckStatus checkStatus, string description)
        {
            CheckStatus = checkStatus;
            Description = description;
        }

        public static HealthCheckResult Unhealthy(string description)
        {
            return new HealthCheckResult(CheckStatus.Unhealthy, description)
            {
                Description = description,
                CheckStatus = CheckStatus.Unhealthy
            };
        }

        public static HealthCheckResult Healthy(string description)
        {
            return new HealthCheckResult(CheckStatus.Healthy, description)
            {
                Description = description,
                CheckStatus = CheckStatus.Healthy
            };
        }

        public static HealthCheckResult Warning(string description)
        {
            return new HealthCheckResult(CheckStatus.Warning, description)
            {
                Description = description,
                CheckStatus = CheckStatus.Warning
            };
        }

        public static HealthCheckResult Unknown(string description)
        {
            return new HealthCheckResult(CheckStatus.Unknown, description)
            {
                Description = description,
                CheckStatus = CheckStatus.Unknown
            };
        }
    }
}
