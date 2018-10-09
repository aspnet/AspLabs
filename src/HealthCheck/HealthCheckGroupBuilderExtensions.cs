using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthChecks
{
    public static class HealthCheckGroupBuilderExtensions
    {
        public static HealthCheckBuilder AddHealthCheckGroup(
            this HealthCheckBuilder builder,
            string groupName,
            Action<HealthCheckBuilder> innerChecks,
            bool strict = false)
        {

            var innerBuilder = new HealthCheckBuilder();
            innerChecks(innerBuilder);

            builder.AddCheck($"Group {groupName}", async () =>
            {
                var result = new List<HealthCheckResult>();
                foreach (var check in innerBuilder.Checks)
                {
                    result.Add(await check.Value());
                }

                if (result.All(x => x.CheckStatus == CheckStatus.Unhealthy))
                {
                    return HealthCheckResult.Unhealthy($"All checks in the group '{groupName}' are unhealthy.", result);
                }
                if (strict && result.Any(x => x.CheckStatus != CheckStatus.Healthy))
                {
                    return HealthCheckResult.Unhealthy($"At least one of the checks in the group '{groupName}' is not healthy.", result);
                }
                if (result.All(x => x.CheckStatus == CheckStatus.Unknown))
                {
                    return HealthCheckResult.Unknown($"All checks in the group '{groupName}' are in an unknown health state.", result);
                }
                if (result.All(x => x.CheckStatus == CheckStatus.Warning))
                {
                    return HealthCheckResult.Warning($"All checks in the group '{groupName}' are in a warning state.", result);
                }
                if (result.Any(x => x.CheckStatus != CheckStatus.Healthy))
                {
                    return HealthCheckResult.Warning($"Not all checks in the group '{groupName}' are in an healthy state.", result);
                }

                return HealthCheckResult.Healthy($"All checks in the group '{groupName}' are healthy.", result);
            });

            return builder;
        }

    }
}