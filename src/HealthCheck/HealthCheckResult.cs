namespace HealthChecks
{
    public class HealthCheckResult
    {
        private CheckStatus _checkStatus;

        private string _description;

        public CheckStatus CheckStatus
        {
            get { return _checkStatus; }
        }

        public string Description
        {
            get { return _description; }
        }

        public static HealthCheckResult Unhealthy(string description)
        {
            return new HealthCheckResult()
            {
                _description = description,
                _checkStatus = CheckStatus.Unhealthy
            };
        }

        public static HealthCheckResult Healthy(string description)
        {
            return new HealthCheckResult()
            {
                _description = description,
                _checkStatus = CheckStatus.Healthy
            };
        }

        public static HealthCheckResult Warning(string description)
        {
            return new HealthCheckResult()
            {
                _description = description,
                _checkStatus = CheckStatus.Warning
            };
        }

        public static HealthCheckResult Unknown(string description)
        {
            return new HealthCheckResult()
            {
                _description = description,
                _checkStatus = CheckStatus.Unknown
            };
        }
    }
}
