using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthChecks
{
    public class HealthCheckBuilder
    {
        public Dictionary<string, Func<ValueTask<HealthCheckResult>>> Checks { get; private set; }

        public HealthCheckBuilder()
        {
            Checks = new Dictionary<string, Func<ValueTask<HealthCheckResult>>>();
        }

        public HealthCheckBuilder AddCheck(string name, Func<Task<HealthCheckResult>> check)
        {
            Checks.Add(name, () => new ValueTask<HealthCheckResult>(check()));
            return this;
        }

        public HealthCheckBuilder AddCheck(string name, Func<HealthCheckResult> check)
        {
            Checks.Add(name, () => new ValueTask<HealthCheckResult>(check()));
            return this;
        }
    }
}