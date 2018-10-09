Proxy
=====

This project contains an ASP.NET Core middleware which runs proxy forwarding requests to another server.

Usage:
```c#
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProxy(options =>
            {
                options.PrepareRequest = (originalRequest, message) =>
                {
                    message.Headers.Add("X-Forwarded-Host", originalRequest.Host.Host);
                    return Task.FromResult(0);
                };
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets().RunProxy(new Uri("https://example.com"));
        }
    }
```
