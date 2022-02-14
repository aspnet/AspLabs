// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Google.Protobuf.Reflection;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Grpc.Shared.Server;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using MethodOptions = Grpc.Shared.Server.MethodOptions;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests
{
    public class HttpApiServerCallContextTests
    {
        [Fact]
        public void CancellationToken_Get_MatchHttpContextRequestAborted()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var httpContext = CreateHttpContext(cancellationToken: cts.Token);
            var serverCallContext = CreateServerCallContext(httpContext);

            // Act
            var ct = serverCallContext.CancellationToken;

            // Assert
            Assert.Equal(cts.Token, ct);
        }

        [Fact]
        public void RequestHeaders_Get_PopulatedFromHttpContext()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers.Add("TestName", "TestValue");
            httpContext.Request.Headers.Add(":method", "GET");
            httpContext.Request.Headers.Add("grpc-encoding", "identity");
            httpContext.Request.Headers.Add("grpc-timeout", "1S");
            httpContext.Request.Headers.Add("hello-bin", Convert.ToBase64String(new byte[] { 1, 2, 3 }));
            var serverCallContext = CreateServerCallContext(httpContext);

            // Act
            var headers = serverCallContext.RequestHeaders;

            // Assert
            Assert.Equal(2, headers.Count);
            Assert.Equal("testname", headers[0].Key);
            Assert.Equal("TestValue", headers[0].Value);
            Assert.Equal("hello-bin", headers[1].Key);
            Assert.True(headers[1].IsBinary);
            Assert.Equal(new byte[] { 1, 2, 3 }, headers[1].ValueBytes);
        }

        private static DefaultHttpContext CreateHttpContext(CancellationToken cancellationToken = default)
        {
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            httpContext.RequestServices = serviceProvider;
            httpContext.Response.Body = new MemoryStream();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Features.Set<IHttpRequestLifetimeFeature>(new HttpRequestLifetimeFeature(cancellationToken));
            return httpContext;
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

        private static HttpApiServerCallContext CreateServerCallContext(DefaultHttpContext httpContext)
        {
            return new HttpApiServerCallContext(
                httpContext,
                MethodOptions.Create(Enumerable.Empty<GrpcServiceOptions>()),
                new Method<object, object>(
                    MethodType.Unary,
                    "Server",
                    "Method",
                    new Marshaller<object>(o => null, c => null!),
                    new Marshaller<object>(o => null, c => null!)),
                new CallHandlerDescriptorInfo(
                    null,
                    null,
                    false,
                    null,
                    new Dictionary<string, List<FieldDescriptor>>()),
                NullLogger.Instance);
        }
    }
}
