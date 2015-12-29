// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
            CustomServices.Reset();
        }

        [Fact]
        public void GetFilterManager_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            Mock<IWebHookFilterManager> instanceMock = new Mock<IWebHookFilterManager>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookFilterManager)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            IWebHookFilterManager actual = _resolverMock.Object.GetFilterManager();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetFilterManager_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IWebHookFilterManager actual = _resolverMock.Object.GetFilterManager();

            // Assert
            Assert.IsType<WebHookFilterManager>(actual);
        }

        [Fact]
        public void GetFilterManager_ReturnsSameInstance_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IWebHookFilterManager actual1 = _resolverMock.Object.GetFilterManager();
            IWebHookFilterManager actual2 = _resolverMock.Object.GetFilterManager();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetSender_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            Mock<IWebHookSender> instanceMock = new Mock<IWebHookSender>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookSender)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            IWebHookSender actual = _resolverMock.Object.GetSender();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetSender_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IWebHookSender actual = _resolverMock.Object.GetSender();

            // Assert
            Assert.IsType<DataflowWebHookSender>(actual);
        }

        [Fact]
        public void GetSender_ReturnsSameInstance_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IWebHookSender actual1 = _resolverMock.Object.GetSender();
            IWebHookSender actual2 = _resolverMock.Object.GetSender();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetManager_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            Mock<IWebHookManager> instanceMock = new Mock<IWebHookManager>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookManager)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            IWebHookManager actual = _resolverMock.Object.GetManager();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetManager_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IWebHookManager actual = _resolverMock.Object.GetManager();

            // Assert
            Assert.IsType<WebHookManager>(actual);
        }

        [Fact]
        public void GetManager_ReturnsSameInstance_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IWebHookManager actual1 = _resolverMock.Object.GetManager();
            IWebHookManager actual2 = _resolverMock.Object.GetManager();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetStore_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            Mock<IWebHookStore> instanceMock = new Mock<IWebHookStore>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookStore)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            IWebHookStore actual = _resolverMock.Object.GetStore();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetStore_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IWebHookStore actual = _resolverMock.Object.GetStore();

            // Assert
            Assert.IsType<MemoryWebHookStore>(actual);
        }

        [Fact]
        public void GetStore_ReturnsSameInstance_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IWebHookStore actual1 = _resolverMock.Object.GetStore();
            IWebHookStore actual2 = _resolverMock.Object.GetStore();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetFilterProviders_ReturnsDependencyInstances_IfRegistered()
        {
            // Arrange
            Mock<IWebHookFilterProvider> instanceMock = new Mock<IWebHookFilterProvider>();
            List<IWebHookFilterProvider> instances = new List<IWebHookFilterProvider> { instanceMock.Object };
            _resolverMock.Setup(r => r.GetServices(typeof(IWebHookFilterProvider)))
                .Returns(instances)
                .Verifiable();

            // Act
            IEnumerable<IWebHookFilterProvider> actual = _resolverMock.Object.GetFilterProviders();

            // Assert
            Assert.Same(instances, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetFilterProviders_ReturnsDefaultInstances_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IEnumerable<IWebHookFilterProvider> actual = _resolverMock.Object.GetFilterProviders();

            // Assert
            Assert.IsType<WildcardWebHookFilterProvider>(actual.Single());
        }

        [Fact]
        public void GetFilterProviders_ReturnsSameInstances_IfNoneRegistered()
        {
            // Arrange
            _config.InitializeCustomWebHooks();

            // Act
            IEnumerable<IWebHookFilterProvider> actual1 = _resolverMock.Object.GetFilterProviders();
            IEnumerable<IWebHookFilterProvider> actual2 = _resolverMock.Object.GetFilterProviders();

            // Assert
            Assert.Same(actual1, actual2);
        }
    }
}
