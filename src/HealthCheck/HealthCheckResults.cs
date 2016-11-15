using System.Collections.Generic;

namespace HealthChecks
{
    public class HealthCheckResults
    {
        public IList<HealthCheckResult> CheckResults { get; } = new List<HealthCheckResult>();
    }
}
