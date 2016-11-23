namespace HealthChecks
{
    public class HealthCheckResult : IHealthCheckResult
    {
        public string Name {get;set;}

        public CheckStatus CheckStatus { get; }
        public string Description { get; }

        private HealthCheckResult(string name, CheckStatus checkStatus, string description)
        {
            Name = name;
            CheckStatus = checkStatus;
            Description = description;
        }

        public static HealthCheckResult Unhealthy(string description)
        {
            return new HealthCheckResult(description, CheckStatus.Unhealthy, description);
        }

        public static HealthCheckResult Healthy(string description)
        {
            return new HealthCheckResult(description, CheckStatus.Healthy, description);
        }

        public static HealthCheckResult Warning(string description)
        {
            return new HealthCheckResult(description, CheckStatus.Warning, description);
        }

        public static HealthCheckResult Unknown(string description)
        {
            return new HealthCheckResult(description, CheckStatus.Unknown, description);
        }
    }
}
