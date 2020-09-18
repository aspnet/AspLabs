// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ProxyProtocol.Sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    var proxyFeature = context.Features.Get<ProxyProtocolFeature>();
                    if (proxyFeature == null)
                    {
                        await context.Response.WriteAsync("Unable to access the proxy protocol feature. Did the client send that data?");
                        return;
                    }

                    await context.Response.WriteAsync($"Source IP: {proxyFeature.SourceIp}\r\n");
                    await context.Response.WriteAsync($"Destination IP: {proxyFeature.DestinationIp}\r\n");
                    await context.Response.WriteAsync($"Source Port: {proxyFeature.SourcePort}\r\n");
                    await context.Response.WriteAsync($"Destination Port: {proxyFeature.DestinationPort}\r\n");
                    await context.Response.WriteAsync($"Link Id: {proxyFeature.LinkId}\r\n");
                });
            });
        }
    }
}
