using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HealthChecks
{
    public static class HealthCheckBuilderExtensions
    {
        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url)
        {
            builder.AddCheck($"UrlCheck ({url})", async () => {
                var healthCheckResult = new HealthCheckResult
                {
                    Success = false,
                    CheckStatus = CheckStatus.Failed,
                    CheckType = "url",
                    Description = $"UrlCheck: {url}"
                };

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                var response = await httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    healthCheckResult.CheckStatus = CheckStatus.Ok;
                    healthCheckResult.Success = true;
                };

                return healthCheckResult;

            });
            return builder;
        }

        public static HealthCheckBuilder AddVirtualMemorySizeCheck(this HealthCheckBuilder builder, long maxSize)
        {
            builder.AddCheck($"VirtualMemorySize ({maxSize})", () =>
            {
                var healthCheckResult = new HealthCheckResult
                {
                    Success = false,
                    CheckStatus = CheckStatus.Failed,
                    CheckType = "memory",
                    Description = $"AddVirtualMemorySizeCheck, maxSize: {maxSize}"
                };

                if (Process.GetCurrentProcess().VirtualMemorySize64 <= maxSize)
                {
                    healthCheckResult.Success = true;
                    healthCheckResult.CheckStatus = CheckStatus.Ok;
                }

                return healthCheckResult;
            });
            
            return builder;
        }

        public static HealthCheckBuilder AddWorkingSetCheck(this HealthCheckBuilder builder, long maxSize)
        {
            builder.AddCheck($"WorkingSet64 ({maxSize})", () =>
            {
                var healthCheckResult = new HealthCheckResult
                {
                    Success = false,
                    CheckStatus = CheckStatus.Failed,
                    CheckType = "memory",
                    Description = $"AddVirtualMemorySizeCheck, maxSize: {maxSize}"
                };

                if (Process.GetCurrentProcess().WorkingSet64 <= maxSize)
                {
                    healthCheckResult.Success = true;
                    healthCheckResult.CheckStatus = CheckStatus.Ok;
                }

                return healthCheckResult;
            });

            return builder;
        }

        public static HealthCheckBuilder AddPrivateMemorySizeCheck(this HealthCheckBuilder builder, long maxSize)
        {
            builder.AddCheck($"PrivateMemorySize64 ({maxSize})", () =>
            {
                var healthCheckResult = new HealthCheckResult
                {
                    Success = false,
                    CheckStatus = CheckStatus.Failed,
                    CheckType = "memory",
                    Description = $"AddVirtualMemorySizeCheck, maxSize: {maxSize}"
                };

                if (Process.GetCurrentProcess().PrivateMemorySize64 <= maxSize)
                {
                    healthCheckResult.Success = true;
                    healthCheckResult.CheckStatus = CheckStatus.Ok;
                }

                return healthCheckResult;
            });

            return builder;
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url, Func<HttpResponseMessage, HealthCheckResult> checkFunc)
        {
            builder.AddCheck($"UrlCheck ({url})", async () =>
            {
                var healthCheckResult = new HealthCheckResult
                {
                    Success = false,
                    CheckStatus = CheckStatus.Failed,
                    CheckType = "memory",
                    Description = $"UrlCheck: {url}"
                };

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                var response = await httpClient.GetAsync(url);
                return checkFunc(response);
            });
            return builder;
        }

        //TODO: Move this into a seperate project. Avoid DB dependencies in the main lib.
        //TODO: It is probably better if this is more generic, not SQL specific.
        public static HealthCheckBuilder AddSqlCheck(this HealthCheckBuilder builder, string connectionString)
        {
            builder.AddCheck($"SQL Check:", async ()=>{
                var healthCheckResult = new HealthCheckResult
                {
                    Success = false,
                    CheckStatus = CheckStatus.Failed,
                    CheckType = "database",
                    Description = $"AddSqlCheck: {connectionString}"
                };

                try
                {
                    //TODO: There is probably a much better way to do this.
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using(var command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandText = "SELECT 1";
                            var result = (int) await command.ExecuteScalarAsync();
                            if(result == 1)
                            {
                                healthCheckResult.Success = true;
                                healthCheckResult.CheckStatus = CheckStatus.Ok;
                            }
                            return healthCheckResult;
                        }
                    }
                }
                catch
                {
                    return healthCheckResult;
                }
            });
            return builder;
        }
    }
}