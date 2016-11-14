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
        int _healthCheckPort;
        IHealthCheckService _checkupService;

        public HealthCheckMiddleware(RequestDelegate next, IHealthCheckService checkupService, int port)
        {
            _healthCheckPort = port;
            _checkupService = checkupService;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var connInfo = context.Features.Get<IHttpConnectionFeature>();
            if (connInfo.LocalPort == _healthCheckPort)
            {
                var healthy = await _checkupService.CheckHealthAsync();
                if (healthy)
                {
                    await context.Response.WriteAsync("HealthCheck: OK");
                }
                else
                {
                    context.Response.StatusCode = 502;
                    context.Response.Headers.Add("content-type", "application/json");
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(_checkupService.CheckResults));
                }
                return;
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}