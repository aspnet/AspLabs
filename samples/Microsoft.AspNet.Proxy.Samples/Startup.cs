// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;

namespace Microsoft.AspNet.Proxy
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            const string scheme = "https";
            const string host = "example.com";
            const string port = "443";
            var options = new ProxyOptions()
            {
                Scheme = scheme,
                Host = host,
                Port = port
            };
            app.RunProxy(options);
        }

        public static void Main(string[] args)
        {
            var application = new WebApplicationBuilder()
                .UseConfiguration(WebApplicationConfiguration.GetDefault(args))
                .UseStartup<Startup>()
                .Build();

            application.Run();
        }
    }
}
