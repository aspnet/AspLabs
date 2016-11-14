using System.Collections.Generic;

namespace HealthChecks
{
    public class HealthCheckResults
    {
        private IList<HealthCheckResult> _checkResults { get; set; }

        public HealthCheckResults()
        {
            _checkResults = new List<HealthCheckResult>();
        }

        public IList<HealthCheckResult> CheckResults
        {
            get { return _checkResults; }
        }
    }
}
