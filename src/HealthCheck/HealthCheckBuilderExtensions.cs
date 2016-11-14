using System;
using System.Net;
using System.Net.Http;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HealthChecks
{
    public static class HealthCheckBuilderExtensions
    {

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string group)
        {
            var urls = urlItems.ToList();
            builder.AddCheck($"UrlChecks ({group})", async () => {
                var healthCheckResult = new HealthCheckResult
                {
                    CheckStatus = CheckStatus.Unhealthy,
                };

                var successfulChecks = 0;
                var description = new StringBuilder();
                var httpClient = new HttpClient();

                foreach (var url in urlItems)
                {
                    try
                    {
                        httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                        var response = await httpClient.GetAsync(url);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            successfulChecks++;
                            description.Append($"UrlCheck SUCCESS ({url}) ");
                        }
                        else
                        {
                            description.Append($"UrlCheck FAILED ({url}) ");
                        }
                    }
                    catch
                    {
                        description.Append($"UrlCheck FAILED ({url}) ");
                    }
                }

                if (successfulChecks == urls.Count)
                {
                    healthCheckResult.CheckStatus = CheckStatus.Healthy;
                }
                else if (successfulChecks > 0)
                {
                    healthCheckResult.CheckStatus = CheckStatus.Warning;
                }
                healthCheckResult.Description = description.ToString();

                return healthCheckResult;

            });
            return builder;
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url)
        {
            builder.AddCheck($"UrlCheck ({url})", async () => {
                var healthCheckResult = new HealthCheckResult
                {
                    CheckStatus = CheckStatus.Unhealthy,
                    Description = $"UrlCheck: {url}"
                };

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                var response = await httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    healthCheckResult.CheckStatus = CheckStatus.Healthy;
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
                    CheckStatus = CheckStatus.Unhealthy,
                    Description = $"AddVirtualMemorySizeCheck, maxSize: {maxSize}"
                };

                if (Process.GetCurrentProcess().VirtualMemorySize64 <= maxSize)
                {
                    healthCheckResult.CheckStatus = CheckStatus.Healthy;
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
                    CheckStatus = CheckStatus.Unhealthy,
                    Description = $"AddWorkingSetCheck, maxSize: {maxSize}"
                };

                if (Process.GetCurrentProcess().WorkingSet64 <= maxSize)
                {
                    healthCheckResult.CheckStatus = CheckStatus.Healthy;
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
                    CheckStatus = CheckStatus.Unhealthy,
                    Description = $"AddPrivateMemorySizeCheck, maxSize: {maxSize}"
                };

                if (Process.GetCurrentProcess().PrivateMemorySize64 <= maxSize)
                {
                    healthCheckResult.CheckStatus = CheckStatus.Healthy;
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
                    CheckStatus = CheckStatus.Unhealthy,
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
            builder.AddCheck($"SQL Check:", async () => {
                var healthCheckResult = new HealthCheckResult
                {
                    CheckStatus = CheckStatus.Unhealthy,
                    Description = $"AddSqlCheck: {connectionString}"
                };

                try
                {
                    //TODO: There is probably a much better way to do this.
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandText = "SELECT 1";
                            var result = (int)await command.ExecuteScalarAsync();
                            if (result == 1)
                            {
                                healthCheckResult.CheckStatus = CheckStatus.Healthy;
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