// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Count;
using Greet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Grpc.Swagger.Tests.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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

        [Fact]
        public void AddGrpcSwagger_GrpcServiceWithGroupName_FilteredByGroup()
        {
            // Arrange & Act
            var services = new ServiceCollection();
            services.AddGrpcSwagger();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "My API", Version = "v2" });
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
                c.MapGrpcService<CounterService>();
            });

            var swaggerGenerator = serviceProvider.GetRequiredService<ISwaggerProvider>();

            // Assert 1
            var swagger = swaggerGenerator.GetSwagger("v1");
            Assert.Single(swagger.Paths);
            Assert.True(swagger.Paths["/v1/greeter/{name}"].Operations.ContainsKey(OperationType.Get));

            // Assert 2
            swagger = swaggerGenerator.GetSwagger("v2");
            Assert.Equal(2, swagger.Paths.Count);
            Assert.True(swagger.Paths["/v1/greeter/{name}"].Operations.ContainsKey(OperationType.Get));
            Assert.True(swagger.Paths["/v1/add/{value1}/{value2}"].Operations.ContainsKey(OperationType.Get));
        }
        
        [Fact]
        public void AddGrpcSwagger_GrpcServiceWithQuery_ResolveQueryParameterDescriptorsTest()
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
                c.MapGrpcService<ParametersService>();
            });

            var swaggerGenerator = serviceProvider.GetRequiredService<ISwaggerProvider>();
            var swagger = swaggerGenerator.GetSwagger("v1");

            // Base Assert
            Assert.NotNull(swagger);

            // Assert 1
            var path = swagger.Paths["/v1/parameters1"];
            Assert.True(path.Operations.ContainsKey(OperationType.Get));
            Assert.True(path.Operations.First().Value.Parameters.Count == 2);
            Assert.True(path.Operations.First().Value.Parameters.ElementAt(0).In == ParameterLocation.Query);
            Assert.True(path.Operations.First().Value.Parameters.ElementAt(1).In == ParameterLocation.Query);
            
            // Assert 2
            path = swagger.Paths["/v1/parameters2/{parameter_int}"];
            Assert.True(path.Operations.ContainsKey(OperationType.Get));
            Assert.True(path.Operations.First().Value.Parameters.Count == 2);
            Assert.True(path.Operations.First().Value.Parameters.ElementAt(0).In == ParameterLocation.Path);
            Assert.True(path.Operations.First().Value.Parameters.ElementAt(1).In == ParameterLocation.Query);
            
            // Assert 3
            path = swagger.Paths["/v1/parameters3/{parameter_one}"];
            Assert.True(path.Operations.ContainsKey(OperationType.Post));
            Assert.True(path.Operations.First().Value.Parameters.Count == 3);
            Assert.True(path.Operations.First().Value.Parameters.ElementAt(0).In == ParameterLocation.Path);
            Assert.True(path.Operations.First().Value.Parameters.ElementAt(1).In == ParameterLocation.Query);
            Assert.True(path.Operations.First().Value.Parameters.ElementAt(2).In == ParameterLocation.Query);
            // body with one parameter
            Assert.NotNull(path.Operations.First().Value.RequestBody);
            Assert.True(swagger.Components.Schemas["RequestBody"].Properties.Count == 1);
            
            // Assert 4
            path = swagger.Paths["/v1/parameters4/{parameter_two}"];
            Assert.True(path.Operations.ContainsKey(OperationType.Post));
            Assert.True(path.Operations.First().Value.Parameters.Count == 1);
            Assert.True(path.Operations.First().Value.Parameters.ElementAt(0).In == ParameterLocation.Path);
            // body with four parameters
            Assert.NotNull(path.Operations.First().Value.RequestBody);
            Assert.True(swagger.Components.Schemas["RequestTwo"].Properties.Count == 4);
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

        [ApiExplorerSettings(GroupName = "v2")]
        private class CounterService : Counter.CounterBase
        {
        }
    }
}
