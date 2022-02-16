// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IntegrationTestsWebsite;
using Microsoft.AspNetCore.Grpc.HttpApi.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Grpc.HttpApi.IntegrationTests
{
    public class IntegrationTestBase : IClassFixture<GrpcTestFixture<Startup>>, IDisposable
    {
        private HttpClient? _channel;
        private IDisposable? _testContext;

        protected GrpcTestFixture<Startup> Fixture { get; set; }

        protected ILoggerFactory LoggerFactory => Fixture.LoggerFactory;

        protected HttpClient Channel => _channel ??= CreateChannel();

        protected HttpClient CreateChannel()
        {
            return new HttpClient(Fixture.Handler);
        }

        public IntegrationTestBase(GrpcTestFixture<Startup> fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            _testContext = Fixture.GetTestContext(outputHelper);
        }

        public void Dispose()
        {
            _testContext?.Dispose();
            _channel = null;
        }
    }

}
