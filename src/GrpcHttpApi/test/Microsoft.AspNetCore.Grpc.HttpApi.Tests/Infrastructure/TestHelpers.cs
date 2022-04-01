// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Reflection;
using Grpc.AspNetCore.Server;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Infrastructure
{
    internal static class TestHelpers
    {
        public static DefaultHttpContext CreateHttpContext(CancellationToken cancellationToken = default, Stream? bodyStream = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<HttpApiGreeterService>();
            serviceCollection.AddSingleton(typeof(IGrpcInterceptorActivator<>), typeof(TestInterceptorActivator<>));
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            httpContext.RequestServices = serviceProvider;
            httpContext.Response.Body = bodyStream ?? new MemoryStream();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Features.Set<IHttpRequestLifetimeFeature>(new HttpRequestLifetimeFeature(cancellationToken));
            return httpContext;
        }

        private class TestInterceptorActivator<T> : IGrpcInterceptorActivator<T> where T : Interceptor
        {
            public GrpcActivatorHandle<Interceptor> Create(IServiceProvider serviceProvider, InterceptorRegistration interceptorRegistration)
            {
                return new GrpcActivatorHandle<Interceptor>(Activator.CreateInstance<T>(), created: true, state: null);
            }

            public ValueTask ReleaseAsync(GrpcActivatorHandle<Interceptor> interceptor)
            {
                return default;
            }
        }

        private class HttpRequestLifetimeFeature : IHttpRequestLifetimeFeature
        {
            public HttpRequestLifetimeFeature(CancellationToken cancellationToken)
            {
                RequestAborted = cancellationToken;
            }

            public CancellationToken RequestAborted { get; set; }

            public void Abort()
            {
            }
        }

        public static CallHandlerDescriptorInfo CreateDescriptorInfo(
            FieldDescriptor? responseBodyDescriptor = null,
            Dictionary<string, List<FieldDescriptor>>? routeParameterDescriptors = null,
            MessageDescriptor? bodyDescriptor = null,
            bool? bodyDescriptorRepeated = null,
            List<FieldDescriptor>? bodyFieldDescriptors = null)
        {
            return new CallHandlerDescriptorInfo(
                responseBodyDescriptor,
                bodyDescriptor,
                bodyDescriptorRepeated ?? false,
                bodyFieldDescriptors,
                routeParameterDescriptors ?? new Dictionary<string, List<FieldDescriptor>>());
        }
    }
}
