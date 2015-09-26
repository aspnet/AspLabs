// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks.Config;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Extensions
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
            WebHooksConfig.Initialize(_config);
            ReceiverServices.Reset();
        }

        [Fact]
        public void GetReceiverManager_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            Mock<IWebHookReceiverManager> instanceMock = new Mock<IWebHookReceiverManager>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookReceiverManager)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            IWebHookReceiverManager actual = _resolverMock.Object.GetReceiverManager();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetReceiverManager_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Act
            IWebHookReceiverManager actual = _resolverMock.Object.GetReceiverManager();

            // Assert
            Assert.IsType<WebHookReceiverManager>(actual);
        }

        [Fact]
        public void GetReceiverManager_ReturnsSameInstance_IfNoneRegistered()
        {
            // Act
            IWebHookReceiverManager actual1 = _resolverMock.Object.GetReceiverManager();
            IWebHookReceiverManager actual2 = _resolverMock.Object.GetReceiverManager();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetHandlerSorter_ReturnsDependencyInstance_IfRegistered()
        {
            // Arrange
            Mock<IWebHookHandlerSorter> instanceMock = new Mock<IWebHookHandlerSorter>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookHandlerSorter)))
                .Returns(instanceMock.Object)
                .Verifiable();

            // Act
            IWebHookHandlerSorter actual = _resolverMock.Object.GetHandlerSorter();

            // Assert
            Assert.Same(instanceMock.Object, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetHandlerSorter_ReturnsDefaultInstance_IfNoneRegistered()
        {
            // Act
            IWebHookHandlerSorter actual = _resolverMock.Object.GetHandlerSorter();

            // Assert
            Assert.IsType<WebHookHandlerSorter>(actual);
        }

        [Fact]
        public void GetHandlerSorter_ReturnsSameInstance_IfNoneRegistered()
        {
            // Act
            IWebHookHandlerSorter actual1 = _resolverMock.Object.GetHandlerSorter();
            IWebHookHandlerSorter actual2 = _resolverMock.Object.GetHandlerSorter();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetReceivers_ReturnsDependencyInstances_IfRegistered()
        {
            // Arrange
            Mock<IWebHookReceiver> instanceMock = new Mock<IWebHookReceiver>();
            List<IWebHookReceiver> instances = new List<IWebHookReceiver> { instanceMock.Object };
            _resolverMock.Setup(r => r.GetServices(typeof(IWebHookReceiver)))
                .Returns(instances)
                .Verifiable();

            // Act
            IEnumerable<IWebHookReceiver> actual = _resolverMock.Object.GetReceivers();

            // Assert
            Assert.Same(instances, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetReceivers_ReturnsSameInstances_IfNoneRegistered()
        {
            // Act
            IEnumerable<IWebHookReceiver> actual1 = _resolverMock.Object.GetReceivers();
            IEnumerable<IWebHookReceiver> actual2 = _resolverMock.Object.GetReceivers();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetHandlers_ReturnsDependencyInstances_IfRegistered()
        {
            // Arrange
            Mock<IWebHookHandler> instanceMock = new Mock<IWebHookHandler>();
            List<IWebHookHandler> instances = new List<IWebHookHandler> { instanceMock.Object };
            _resolverMock.Setup(r => r.GetServices(typeof(IWebHookHandler)))
                .Returns(instances)
                .Verifiable();

            // Act
            IEnumerable<IWebHookHandler> actual = _resolverMock.Object.GetHandlers();

            // Assert
            Assert.Equal(instances, actual);
            instanceMock.Verify();
        }

        [Fact]
        public void GetHandlers_ReturnsSameInstances_IfNoneRegistered()
        {
            // Act
            IEnumerable<IWebHookHandler> actual1 = _resolverMock.Object.GetHandlers();
            IEnumerable<IWebHookHandler> actual2 = _resolverMock.Object.GetHandlers();

            // Assert
            Assert.Equal(actual1, actual2);
        }
    }
}
