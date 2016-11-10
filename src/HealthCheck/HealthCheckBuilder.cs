using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace HealthChecks
{
    public class HealthCheckBuilder
    {
        public Dictionary<string, Func<ValueTask<bool>>> Checks { get; private set; }

        public HealthCheckBuilder()
        {
            Checks = new Dictionary<string, Func<ValueTask<bool>>>();
        }

        public HealthCheckBuilder AddCheck(string name, Func<Task<bool>> check)
        {
            Checks.Add(name, () => new ValueTask<bool>(check()));
            return this;
        }

        public HealthCheckBuilder AddCheck(string name, Func<bool> check)
        {
            Checks.Add(name, () => new ValueTask<bool>(check()));
            return this;
        }
    }
}