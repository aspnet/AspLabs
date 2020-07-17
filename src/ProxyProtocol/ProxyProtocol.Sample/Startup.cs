using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections.Features;
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
                    var connectionItems = context.Features.Get<IConnectionItemsFeature>()?.Items;
                    if (connectionItems == null)
                    {
                        await context.Response.WriteAsync("Unable to access the connection items. Are you using Kestrel?");
                        return;
                    }

                    await context.Response.WriteAsync($"Source IP: {connectionItems[ProxyProtocol.SourceIPAddressKey]}\r\n");
                    await context.Response.WriteAsync($"Destination IP: {connectionItems[ProxyProtocol.DestinationIPAddressKey]}\r\n");
                    await context.Response.WriteAsync($"Source Port: {connectionItems[ProxyProtocol.SourcePortKey]}\r\n");
                    await context.Response.WriteAsync($"Destination Port: {connectionItems[ProxyProtocol.DestinationPortKey]}\r\n");
                    await context.Response.WriteAsync($"Link Id: {connectionItems[ProxyProtocol.LinkIdKey]}\r\n");
                });
            });
        }
    }
}
