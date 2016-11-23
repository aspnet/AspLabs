using System.Collections.Generic;
using System.Linq;

namespace HealthChecks
{
    public class HealthCheckResults
    {
        public IList<IHealthCheckResult> CheckResults { get; } = new List<IHealthCheckResult>();

        public bool IsHealthy
        {
            get
            {
                return !CheckResults.Any(x => x.CheckStatus == CheckStatus.Unhealthy);
            }
        }
    }
}
