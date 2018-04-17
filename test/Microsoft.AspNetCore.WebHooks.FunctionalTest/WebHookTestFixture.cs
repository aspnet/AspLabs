// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.WebHooks.FunctionalTest
{
    public class WebHookTestFixture<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        public WebHookTestFixture()
            : base()
        {
            ClientOptions.BaseAddress = new Uri("https://localhost");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
                services.Configure<MvcCompatibilityOptions>(
                    options => options.CompatibilityVersion = CompatibilityVersion.Latest);
            });
        }
    }
}
