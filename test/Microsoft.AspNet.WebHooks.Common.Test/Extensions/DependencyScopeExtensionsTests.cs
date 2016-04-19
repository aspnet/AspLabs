// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
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
            CommonServices.Reset();
        }

        [Fact]
        public void GetLogger_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            Mock<ILogger> instanceMock = new Mock<ILogger>();
            _resolverMock.Setup(r => r.GetService(typeof(ILogger)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            ILogger actual = _resolverMock.Object.GetLogger();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetLogger_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Act
            ILogger actual = _resolverMock.Object.GetLogger();

            // Assert
            Assert.IsType<TraceLogger>(actual);
        }

        [Fact]
        public void GetSettings_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            SettingsDictionary instance = new SettingsDictionary();
            instance["key"] = "value";
            _resolverMock.Setup(r => r.GetService(typeof(SettingsDictionary)))
                .Returns(instance)
                .Verifiable();

            // Act
            SettingsDictionary actual = _resolverMock.Object.GetSettings();

            // Assert
            Assert.Same(instance, actual);
            _resolverMock.Verify();
        }

        [Fact]
        public void GetSettings_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Act
            SettingsDictionary actual = _resolverMock.Object.GetSettings();

            // Assert
            Assert.IsType<SettingsDictionary>(actual);
        }

        [Fact]
        public void GetSettings_ReturnsDefaultInstance_IfEmptyDictionaryRegistered()
        {
            // Arrange
            SettingsDictionary instance = new SettingsDictionary();
            instance.Clear();
            _resolverMock.Setup(r => r.GetService(typeof(SettingsDictionary)))
                .Returns(instance)
                .Verifiable();

            // Act
            SettingsDictionary actual = _resolverMock.Object.GetSettings();

            // Assert
            Assert.NotSame(instance, actual);
            _resolverMock.Verify();
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
