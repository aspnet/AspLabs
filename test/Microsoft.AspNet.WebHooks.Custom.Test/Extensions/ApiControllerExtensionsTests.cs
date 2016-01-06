// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks;
using Moq;
using Xunit;

namespace System.Web.Http
{
    public class ApiControllerExtensionsTests
    {
        private Mock<IWebHookManager> _managerMock;
        private Mock<IDependencyResolver> _resolverMock;
        private IWebHookUser _user;
        private HttpRequestContext _context;
        private ApiController _controller;
        private IPrincipal _principal;

        public ApiControllerExtensionsTests()
        {
            HttpConfiguration config = new HttpConfiguration();
            _user = new WebHookUser();

            _managerMock = new Mock<IWebHookManager>();
            _resolverMock = new Mock<IDependencyResolver>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookManager)))
                .Returns(_managerMock.Object)
                .Verifiable();

            config.DependencyResolver = _resolverMock.Object;

            ClaimsIdentity identity = new ClaimsIdentity();
            Claim claim = new Claim(ClaimTypes.Name, "TestUser");
            identity.AddClaim(claim);
            _principal = new ClaimsPrincipal(identity);

            _context = new HttpRequestContext()
            {
                Configuration = config,
                Principal = _principal
            };
            _controller = new TestController()
            {
                RequestContext = _context
            };
        }

        [Fact]
        public async Task NotifyAsync_HandlesNullData()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            await _controller.NotifyAsync("a1", data: null);

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesNullDataAndNullPredicate()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            await _controller.NotifyAsync("a1", data: null, predicate: null);

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesNoNotifications()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            int actual = await _controller.NotifyAsync();

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public async Task NotifyAsync_HandlesNoNotificationsAndNoPredicate()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            int actual = await _controller.NotifyAsync(notifications: new NotificationDictionary[0], predicate: null);

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public async Task NotifyAsync_HandlesEmptyDictionaryData()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            await _controller.NotifyAsync("a1", new Dictionary<string, object>());

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesEmptyDictionaryDataAndNoPredicate()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            await _controller.NotifyAsync("a1", new Dictionary<string, object>());

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesEmptyObjectData()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            await _controller.NotifyAsync("a1", new object());

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesEmptyObjectDataAndNoPredicate()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            await _controller.NotifyAsync("a1", new object(), predicate: null);

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesNonemptyDictionaryData()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            await _controller.NotifyAsync("a1", new Dictionary<string, object>() { { "d1", "v1" } });

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1" && (string)n.Single()["d1"] == "v1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesNonemptyDictionaryDataAndPredicate()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();
            Func<WebHook, string, bool> predicate = (w, s) => true;

            // Act
            await _controller.NotifyAsync("a1", new Dictionary<string, object>() { { "d1", "v1" } }, predicate);

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1" && (string)n.Single()["d1"] == "v1"), predicate));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesNonemptyObjectData()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            // Act
            await _controller.NotifyAsync("a1", new { d1 = "v1" });

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1" && (string)n.Single()["d1"] == "v1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesNonemptyObjectDataAndPredicate()
        {
            // Arrange
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();
            Func<WebHook, string, bool> predicate = (w, s) => true;

            // Act
            await _controller.NotifyAsync("a1", new { d1 = "v1" }, predicate);

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1" && (string)n.Single()["d1"] == "v1"), predicate));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesNullData()
        {
            // Act
            await _controller.NotifyAllAsync("a1", data: null);

            // Assert
            _managerMock.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesNullDataAndNullPredicate()
        {
            // Act
            await _controller.NotifyAllAsync("a1", data: null, predicate: null);

            // Assert
            _managerMock.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesNoNotifications()
        {
            // Act
            int actual = await _controller.NotifyAllAsync(notifications: new NotificationDictionary[0]);

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesNoNotificationsAndNoPredicate()
        {
            // Act
            int actual = await _controller.NotifyAllAsync(notifications: new NotificationDictionary[0], predicate: null);

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesEmptyDictionaryData()
        {
            // Act
            await _controller.NotifyAllAsync("a1", new Dictionary<string, object>());

            // Assert
            _managerMock.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesEmptyDictionaryDataAndNoPredicate()
        {
            // Act
            await _controller.NotifyAllAsync("a1", new Dictionary<string, object>());

            // Assert
            _managerMock.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesEmptyObjectData()
        {
            // Act
            await _controller.NotifyAllAsync("a1", new object());

            // Assert
            _managerMock.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesEmptyObjectDataAndNoPredicate()
        {
            // Act
            await _controller.NotifyAllAsync("a1", new object(), predicate: null);

            // Assert
            _managerMock.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesNonemptyDictionaryDataAndPredicate()
        {
            // Arrange
            Func<WebHook, string, bool> predicate = (w, s) => true;

            // Act
            await _controller.NotifyAllAsync("a1", new Dictionary<string, object>() { { "d1", "v1" } }, predicate);

            // Assert
            _managerMock.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1" && (string)n.Single()["d1"] == "v1"), predicate));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAllAsync_HandlesNonemptyObjectDataAndPredicate()
        {
            // Arrange
            Func<WebHook, string, bool> predicate = (w, s) => true;

            // Act
            await _controller.NotifyAllAsync("a1", new { d1 = "v1" }, predicate);

            // Assert
            _managerMock.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => n.Single().Action == "a1" && (string)n.Single()["d1"] == "v1"), predicate));
            _resolverMock.Verify();
        }

        private class TestController : ApiController
        {
        }
    }
}
