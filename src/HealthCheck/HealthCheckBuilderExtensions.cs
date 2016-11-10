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
    public static class HealthCheckBuilderExtensions
    {
        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url)
        {
            builder.AddCheck($"UrlCheck ({url})", async () => {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);
                return response.StatusCode == HttpStatusCode.OK;
            });
            return builder;
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url, Func<HttpResponseMessage, bool> checkFunc)
        {
            builder.AddCheck($"UrlCheck ({url})", async () =>{
                var httpClient = new HttpClient();
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
                try
                {
                    //TODO: There is probably a much better way to do this.
                    using(var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using(var command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandText = "SELECT 1";
                            var result = (int) await command.ExecuteScalarAsync();
                            return result == 1;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            });
            return builder;
        }
    }
}