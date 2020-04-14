// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Greet;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.AspNetCore.Grpc.Swagger.Tests
{
    [TestFixture]
    public class GrpcSwaggerServiceExtensionsTests
    {
        [Test]
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
            Assert.IsNotNull(swagger);
            Assert.AreEqual(1, swagger.Paths.Count);

            var path = swagger.Paths["/v1/greeter/{name}"];
            Assert.IsTrue(path.Operations.ContainsKey(OperationType.Get));
        }

        private class GreeterService : Greeter.GreeterBase
        {
        }
    }
}
