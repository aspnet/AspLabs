// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProxyProtocol.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var logger = options.ApplicationServices.GetRequiredService<ILogger<Program>>();
                        options.ListenAnyIP(5001, listenOptions =>
                        {
                            listenOptions.UseHttps();
                            listenOptions.Use(async (connectionContext, next) =>
                            {
                                await ProxyProtocol.ProcessAsync(connectionContext, next, logger);
                            });
                        });
                        options.ListenAnyIP(5000, listenOptions =>
                        {
                            listenOptions.Use(async (connectionContext, next) =>
                            {
                                await ProxyProtocol.ProcessAsync(connectionContext, next, logger);
                            });
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
