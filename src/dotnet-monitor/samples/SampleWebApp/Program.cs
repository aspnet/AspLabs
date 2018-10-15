// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.Server;
using Microsoft.Extensions.Logging;

namespace SampleWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DiagnosticServer.Start();
            Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    // Just log to EventSource
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddEventSourceLogger();
                })
                .UseStartup<Startup>();
    }
}
