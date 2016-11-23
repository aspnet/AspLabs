using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthChecks
{
    public class HealthCheckBuilder
    {
        public Dictionary<string, Func<ValueTask<IHealthCheckResult>>> Checks { get; private set; }

        public HealthCheckBuilder()
        {
            Checks = new Dictionary<string, Func<ValueTask<IHealthCheckResult>>>();
        }

        public HealthCheckBuilder AddCheck(string name, Func<Task<IHealthCheckResult>> check)
        {
            Checks.Add(name, () => new ValueTask<IHealthCheckResult>(check()));
            return this;
        }

        public HealthCheckBuilder AddCheck(string name, Func<IHealthCheckResult> check)
        {
            Checks.Add(name, () => new ValueTask<IHealthCheckResult>(check()));
            return this;
        }
    }
}