using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthChecks
{
    public interface IHealthCheckService
    {
        Task<HealthCheckResults> CheckHealthAsync();
    }
}