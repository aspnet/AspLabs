// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.AspNetCore.Proxy
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            const string scheme = "https";
            const string host = "example.com";
            const string port = "443";
            app.RunProxy(new ProxyOptions
            {
                Scheme = scheme,
                Host = host,
                Port = port
            });
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseDefaultHostingConfiguration(args)
                .UseKestrel()
                .UseIISPlatformHandlerUrl()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}

