using System.Collections.Generic;

namespace HealthChecks
{
    public class HealthCheckResults
    {
        public HealthCheckResults()
        {
            CheckResults = new List<HealthCheckResult>();
        }

        public IList<HealthCheckResult> CheckResults { get; set; }
    }
}
