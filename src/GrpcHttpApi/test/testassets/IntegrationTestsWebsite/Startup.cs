// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Grpc.AspNetCore.Server.Model;
using IntegrationTestsWebsite.Infrastructure;
using IntegrationTestsWebsite.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTestsWebsite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddGrpc(options =>
                {
                    options.EnableDetailedErrors = true;
                });
            services.AddGrpcHttpApi();
            services.AddHttpContextAccessor();

            // When the site is run from the test project these types will be injected
            // This will add a default types if the site is run standalone
            services.TryAddSingleton<DynamicEndpointDataSource>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IServiceMethodProvider<DynamicService>, DynamicServiceModelProvider>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.DataSources.Add(endpoints.ServiceProvider.GetRequiredService<DynamicEndpointDataSource>());
            });
        }
    }
}
