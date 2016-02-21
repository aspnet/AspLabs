// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks.Services;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    [Collection("ConfigCollection")]
    public class DependencyScopeExtensionsTests
    {
        private readonly Mock<IDependencyScope> _resolverMock;
        private readonly HttpConfiguration _config;

        public DependencyScopeExtensionsTests()
        {
            _resolverMock = new Mock<IDependencyScope>();
            _config = new HttpConfiguration();
            CustomApiServices.Reset();
        }

        [Fact]
        public void GetRegistrars_ReturnsDependencyInstances_IfRegistered()
        {
            // Arrange
            Mock<IWebHookRegistrar> instanceMock = new Mock<IWebHookRegistrar>();
            List<IWebHookRegistrar> instances = new List<IWebHookRegistrar> { instanceMock.Object };
            _resolverMock.Setup(r => r.GetServices(typeof(IWebHookRegistrar)))
                .Returns(instances)
                .Verifiable();

            // Act
            IEnumerable<IWebHookRegistrar> actual = _resolverMock.Object.GetRegistrars();

            // Assert
            Assert.Same(instances, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetRegistrars_ReturnsDefaultInstances_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IEnumerable<IWebHookRegistrar> actual = _resolverMock.Object.GetRegistrars();

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetRegistrars_ReturnsSameInstances_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IEnumerable<IWebHookRegistrar> actual1 = _resolverMock.Object.GetRegistrars();
            IEnumerable<IWebHookRegistrar> actual2 = _resolverMock.Object.GetRegistrars();

            // Assert
            Assert.Same(actual1, actual2);
        }
    }
}
