using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;

namespace HealthChecks
{
    public class HealthCheckMiddleware
    {
        RequestDelegate _next;
        int? _healthCheckPort;
        string _path;
        IHealthCheckService _healthCheckService;

        //TODO: This would be some sort of options rather than this presumably.
        public HealthCheckMiddleware(RequestDelegate next, IHealthCheckService checkupService, string path)
            : this(next, checkupService, path, null)
        {

        }

        public HealthCheckMiddleware(RequestDelegate next, IHealthCheckService checkupService, string path, int? port)
        {
            _healthCheckPort = port;
            _path = path;
            _healthCheckService = checkupService;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if(_healthCheckPort.HasValue)
            {
                var connInfo = context.Features.Get<IHttpConnectionFeature>();
                if (connInfo.LocalPort != _healthCheckPort)
                {
                    await _next.Invoke(context);
                    return;
                }
            }

            if (context.Request.Path == _path)
            {
                var result = await _healthCheckService.CheckHealthAsync();
                if (!result.IsHealthy)
                {
                    context.Response.StatusCode = 502;
                }

                context.Response.Headers.Add("content-type", "application/json");
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));

                return;
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}