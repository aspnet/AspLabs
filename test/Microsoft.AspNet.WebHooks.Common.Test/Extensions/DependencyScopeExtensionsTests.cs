// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    [Collection("ConfigCollection")]
    public class DependencyScopeExtensionsTests
    {
        private Mock<IDependencyScope> _resolverMock;
        private IDependencyScope _resolver;
        private Service _service1, _service2;

        public DependencyScopeExtensionsTests()
        {
            _resolverMock = new Mock<IDependencyScope>();
            _resolver = _resolverMock.Object;
            _service1 = new Service();
            _service2 = new Service();
        }

        [Fact]
        public void GetService_ReturnsNull_IfServiceNotFound()
        {
            Service actual = _resolver.GetService<Service>();
            Assert.Null(actual);
        }

        [Fact]
        public void GetService_ReturnsService()
        {
            _resolverMock.Setup<object>(r => r.GetService(typeof(Service)))
                .Returns(_service1)
                .Verifiable();

            Service actual = _resolver.GetService<Service>();

            _resolverMock.Verify();
            Assert.Same(_service1, actual);
        }

        [Fact]
        public void GetServices_ReturnsEmpty_IfServiceNotFound()
        {
            IEnumerable<Service> actual = _resolver.GetServices<Service>();
            Assert.Empty(actual);
        }

        [Fact]
        public void GetServices_ReturnsServices()
        {
            var services = new[] { _service1, _service2 };
            _resolverMock.Setup<object>(r => r.GetServices(typeof(Service)))
                .Returns(services)
                .Verifiable();

            IEnumerable<Service> actual = _resolver.GetServices<Service>();

            _resolverMock.Verify();
            Assert.Equal(services, actual);
        }

        private class Service
        {
        }
    }
}
