// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Grpc.AspNetCore.Server.Model;
using Grpc.AspNetCore.Server.Tests.TestObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Grpc.HttpApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace Grpc.AspNetCore.Server.Tests.HttpApi
{
    [TestFixture]
    public class HttpApiServiceMethodProviderTests
    {
        [Test]
        public void AddMethod_OptionGet_ResolveMethod()
        {
            // Arrange & Act
            var endpoints = MapEndpoints<HttpApiGreeterService>();

            // Assert
            var endpoint = FindGrpcEndpoint(endpoints, nameof(HttpApiGreeterService.SayHello));

            Assert.AreEqual("GET", endpoint.Metadata.GetMetadata<IHttpMethodMetadata>().HttpMethods.Single());
            Assert.AreEqual("/v1/greeter/{name}", endpoint.RoutePattern.RawText);
            Assert.AreEqual(1, endpoint.RoutePattern.Parameters.Count);
            Assert.AreEqual("name", endpoint.RoutePattern.Parameters[0].Name);
        }

        [Test]
        public void AddMethod_OptionCustom_ResolveMethod()
        {
            // Arrange & Act
            var endpoints = MapEndpoints<HttpApiGreeterService>();

            // Assert
            var endpoint = FindGrpcEndpoint(endpoints, nameof(HttpApiGreeterService.Custom));

            Assert.AreEqual("/v1/greeter/{name}", endpoint.RoutePattern.RawText);
            Assert.AreEqual("HEAD", endpoint.Metadata.GetMetadata<IHttpMethodMetadata>().HttpMethods.Single());
        }

        [Test]
        public void AddMethod_OptionAdditionalBindings_ResolveMethods()
        {
            // Arrange & Act
            var endpoints = MapEndpoints<HttpApiGreeterService>();

            var matchedEndpoints = FindGrpcEndpoints(endpoints, nameof(HttpApiGreeterService.AdditionalBindings));

            // Assert
            Assert.AreEqual(2, matchedEndpoints.Count);

            var getMethodModel = matchedEndpoints[0];
            Assert.AreEqual("GET", getMethodModel.Metadata.GetMetadata<IHttpMethodMetadata>().HttpMethods.Single());
            Assert.AreEqual("/v1/additional_bindings/{name}", getMethodModel.RoutePattern.RawText);

            var additionalMethodModel = matchedEndpoints[1];
            Assert.AreEqual("DELETE", additionalMethodModel.Metadata.GetMetadata<IHttpMethodMetadata>().HttpMethods.Single());
            Assert.AreEqual("/v1/additional_bindings/{name}", additionalMethodModel.RoutePattern.RawText);
        }

        [Test]
        public void AddMethod_NoOption_ResolveMethod()
        {
            // Arrange & Act
            var endpoints = MapEndpoints<HttpApiGreeterService>();

            // Assert
            var endpoint = FindGrpcEndpoint(endpoints, nameof(HttpApiGreeterService.NoOption));

            Assert.AreEqual("/http_api.HttpApiGreeter/NoOption", endpoint.RoutePattern.RawText);
            Assert.AreEqual("GET", endpoint.Metadata.GetMetadata<IHttpMethodMetadata>().HttpMethods.Single());
        }

        [Test]
        public void AddMethod_BadResponseBody_ThrowError()
        {
            // Arrange & Act
            var ex = Assert.Throws<InvalidOperationException>(() => MapEndpoints<HttpApiInvalidResponseBodyGreeterService>());

            // Assert
            Assert.AreEqual("Error binding gRPC service 'HttpApiInvalidResponseBodyGreeterService'.", ex.Message);
            Assert.AreEqual("Error binding BadResponseBody on HttpApiInvalidResponseBodyGreeterService to HTTP API.", ex.InnerException!.InnerException!.Message);
            Assert.AreEqual("Couldn't find matching field for response body 'NoMatch' on HelloReply.", ex.InnerException!.InnerException!.InnerException!.Message);
        }

        [Test]
        public void AddMethod_BadBody_ThrowError()
        {
            // Arrange & Act
            var ex = Assert.Throws<InvalidOperationException>(() => MapEndpoints<HttpApiInvalidBodyGreeterService>());

            // Assert
            Assert.AreEqual("Error binding gRPC service 'HttpApiInvalidBodyGreeterService'.", ex.Message);
            Assert.AreEqual("Error binding BadBody on HttpApiInvalidBodyGreeterService to HTTP API.", ex.InnerException!.InnerException!.Message);
            Assert.AreEqual("Couldn't find matching field for body 'NoMatch' on HelloRequest.", ex.InnerException!.InnerException!.InnerException!.Message);
        }

        [Test]
        public void AddMethod_BadPattern_ThrowError()
        {
            // Arrange & Act
            var ex = Assert.Throws<InvalidOperationException>(() => MapEndpoints<HttpApiInvalidPatternGreeterService>());

            // Assert
            Assert.AreEqual("Error binding gRPC service 'HttpApiInvalidPatternGreeterService'.", ex.Message);
            Assert.AreEqual("Error binding BadPattern on HttpApiInvalidPatternGreeterService to HTTP API.", ex.InnerException!.InnerException!.Message);
            Assert.AreEqual("Path template must start with /: v1/greeter/{name}", ex.InnerException!.InnerException!.InnerException!.Message);
        }

        private static RouteEndpoint FindGrpcEndpoint(IReadOnlyList<Endpoint> endpoints, string methodName)
        {
            var e = FindGrpcEndpoints(endpoints, methodName).SingleOrDefault();
            if (e == null)
            {
                throw new InvalidOperationException($"Couldn't find gRPC endpoint for method {methodName}.");
            }

            return e;
        }

        private static List<RouteEndpoint> FindGrpcEndpoints(IReadOnlyList<Endpoint> endpoints, string methodName)
        {
            var e = endpoints
                .Where(e => e.Metadata.GetMetadata<GrpcMethodMetadata>()?.Method.Name == methodName)
                .Cast<RouteEndpoint>()
                .ToList();

            return e;
        }

        private class TestEndpointRouteBuilder : IEndpointRouteBuilder
        {
            public ICollection<EndpointDataSource> DataSources { get; }
            public IServiceProvider ServiceProvider { get; }

            public TestEndpointRouteBuilder(IServiceProvider serviceProvider)
            {
                DataSources = new List<EndpointDataSource>();
                ServiceProvider = serviceProvider;
            }

            public IApplicationBuilder CreateApplicationBuilder()
            {
                return new ApplicationBuilder(ServiceProvider);
            }
        }

        private IReadOnlyList<Endpoint> MapEndpoints<TService>()
            where TService : class
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddGrpc();
            serviceCollection.RemoveAll(typeof(IServiceMethodProvider<>));
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<>), typeof(HttpApiServiceMethodProvider<>)));

            IEndpointRouteBuilder endpointRouteBuilder = new TestEndpointRouteBuilder(serviceCollection.BuildServiceProvider());

            endpointRouteBuilder.MapGrpcService<TService>();

            return endpointRouteBuilder.DataSources.Single().Endpoints;
        }
    }
}
