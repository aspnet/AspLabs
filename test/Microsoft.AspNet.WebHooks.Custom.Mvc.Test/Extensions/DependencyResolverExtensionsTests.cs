// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using Moq;
using Xunit;

namespace System.Web.Mvc
{
    public class DependencyResolverExtensionsTests
    {
        private readonly System.Web.Http.HttpConfiguration _config;
        private readonly Mock<IDependencyResolver> _resolverMock;

        public DependencyResolverExtensionsTests()
        {
            _config = new System.Web.Http.HttpConfiguration();
            _resolverMock = new Mock<IDependencyResolver>();
            CustomServices.Reset();
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
            Mock<SettingsDictionary> instanceMock = new Mock<SettingsDictionary>();
            _resolverMock.Setup(r => r.GetService(typeof(SettingsDictionary)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            SettingsDictionary actual = _resolverMock.Object.GetSettings();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
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
            WebHooksConfig.Initialize(_config);

            // Act
            IWebHookFilterManager actual = _resolverMock.Object.GetFilterManager();

            // Assert
            Assert.IsType<WebHookFilterManager>(actual);
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
            // Act
            IWebHookSender actual = _resolverMock.Object.GetSender();

            // Assert
            Assert.IsType<DataflowWebHookSender>(actual);
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
            // Act
            IWebHookManager actual = _resolverMock.Object.GetManager();

            // Assert
            Assert.IsType<WebHookManager>(actual);
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
            // Act
            IWebHookStore actual = _resolverMock.Object.GetStore();

            // Assert
            Assert.IsType<MemoryWebHookStore>(actual);
        }

        [Fact]
        public void GetUser_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            Mock<IWebHookUser> instanceMock = new Mock<IWebHookUser>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            IWebHookUser actual = _resolverMock.Object.GetUser();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetUser_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Act
            IWebHookUser actual = _resolverMock.Object.GetUser();

            // Assert
            Assert.IsType<WebHookUser>(actual);
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
        public void GetFilterProviders_ReturnsSameInstances_IfNoneRegistered()
        {
            // Arrange
            WebHooksConfig.Initialize(_config);

            // Act
            IEnumerable<IWebHookFilterProvider> actual1 = _resolverMock.Object.GetFilterProviders();
            IEnumerable<IWebHookFilterProvider> actual2 = _resolverMock.Object.GetFilterProviders();

            // Assert
            Assert.Same(actual1, actual2);
        }
    }
}
