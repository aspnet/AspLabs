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

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string group, bool strict = false)
        {
            var urls = urlItems.ToList();
            builder.AddCheck($"UrlChecks ({group})", async () => {

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
                    return HealthCheckResult.Healthy(description.ToString());
                }
                else if (successfulChecks > 0)
                {
                    if (strict)
                    {
                        return HealthCheckResult.Unhealthy(description.ToString());
                    }

                    return HealthCheckResult.Warning(description.ToString());
                }

                return HealthCheckResult.Unhealthy(description.ToString());

            });
            return builder;
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url)
        {
            builder.AddCheck($"UrlCheck ({url})", async () => {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                var response = await httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return HealthCheckResult.Healthy($"UrlCheck: {url}");
                };

                return HealthCheckResult.Unhealthy($"UrlCheck: {url}");

            });
            return builder;
        }

        public static HealthCheckBuilder AddVirtualMemorySizeCheck(this HealthCheckBuilder builder, long maxSize)
        {
            builder.AddCheck($"VirtualMemorySize ({maxSize})", () =>
            {
                if (Process.GetCurrentProcess().VirtualMemorySize64 <= maxSize)
                {
                    return HealthCheckResult.Healthy($"AddVirtualMemorySizeCheck, maxSize: {maxSize}");
                }

                return HealthCheckResult.Unhealthy($"AddVirtualMemorySizeCheck, maxSize: {maxSize}");
            });

            return builder;
        }

        public static HealthCheckBuilder AddWorkingSetCheck(this HealthCheckBuilder builder, long maxSize)
        {
            builder.AddCheck($"WorkingSet64 ({maxSize})", () =>
            {
                if (Process.GetCurrentProcess().WorkingSet64 <= maxSize)
                {
                    return HealthCheckResult.Healthy($"AddWorkingSetCheck, maxSize: {maxSize}");
                }

                return HealthCheckResult.Unhealthy($"AddWorkingSetCheck, maxSize: {maxSize}");
            });

            return builder;
        }

        public static HealthCheckBuilder AddPrivateMemorySizeCheck(this HealthCheckBuilder builder, long maxSize)
        {
            builder.AddCheck($"PrivateMemorySize64 ({maxSize})", () =>
            {
                if (Process.GetCurrentProcess().PrivateMemorySize64 <= maxSize)
                {
                    return HealthCheckResult.Healthy($"AddPrivateMemorySizeCheck, maxSize: {maxSize}");
                }

                return HealthCheckResult.Unhealthy($"AddPrivateMemorySizeCheck, maxSize: {maxSize}");
            });

            return builder;
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url, Func<HttpResponseMessage, HealthCheckResult> checkFunc)
        {
            builder.AddCheck($"UrlCheck ({url})", async () =>
            {
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
                                return HealthCheckResult.Healthy($"AddSqlCheck: {connectionString}");
                            }

                            return HealthCheckResult.Unhealthy($"AddSqlCheck: {connectionString}");
                        }
                    }
                }
                catch
                {
                    return HealthCheckResult.Unhealthy($"AddSqlCheck: {connectionString}");
                }
            });

            return builder;
        }
    }
}