// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Greet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace Microsoft.AspNetCore.Grpc.Swagger.Tests
{
    public class GrpcSwaggerServiceExtensionsTests
    {
        [Fact]
        public void AddGrpcSwagger_GrpcServiceRegistered_ReturnSwaggerWithGrpcOperation()
        {
            // Arrange & Act
            var services = new ServiceCollection();
            services.AddGrpcSwagger();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
            services.AddRouting();
            services.AddLogging();
            services.AddSingleton<IWebHostEnvironment, TestWebHostEnvironment>();
            var serviceProvider = services.BuildServiceProvider();
            var app = new ApplicationBuilder(serviceProvider);

            app.UseRouting();
            app.UseEndpoints(c =>
            {
                c.MapGrpcService<GreeterService>();
            });

            var swaggerGenerator = serviceProvider.GetRequiredService<ISwaggerProvider>();
            var swagger = swaggerGenerator.GetSwagger("v1");

            // Assert
            Assert.NotNull(swagger);
            Assert.Single(swagger.Paths);

            var path = swagger.Paths["/v1/greeter/{name}"];
            Assert.True(path.Operations.ContainsKey(OperationType.Get));
        }

        private class TestWebHostEnvironment : IWebHostEnvironment
        {
            public IFileProvider WebRootFileProvider { get; set; } = default!;
            public string WebRootPath { get; set; } = default!;
            public string? ApplicationName { get; set; }
            public IFileProvider? ContentRootFileProvider { get; set; }
            public string? ContentRootPath { get; set; }
            public string? EnvironmentName { get; set; }
        }

        private class GreeterService : Greeter.GreeterBase
        {
        }
    }
}
