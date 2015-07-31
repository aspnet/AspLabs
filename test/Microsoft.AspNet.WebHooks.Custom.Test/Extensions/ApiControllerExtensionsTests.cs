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
        private HttpRequestContext _context;
        private ApiController _controller;
        private IPrincipal _principal;

        public ApiControllerExtensionsTests()
        {
            HttpConfiguration config = new HttpConfiguration();

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
            // Act
            await _controller.NotifyAsync("a1");

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", It.Is<IEnumerable<string>>(i => i.Single() == "a1"), null));
            _resolverMock.Verify();
        }

        [Fact]
        public async Task NotifyAsync_HandlesNonNullData()
        {
            // Arrange
            IEnumerable<string> actions = new[] { "a1" };
            IDictionary<string, object> data = new Dictionary<string, object>();

            // Act
            await _controller.NotifyAsync(actions, data);

            // Assert
            _managerMock.Verify(m => m.NotifyAsync("TestUser", actions, data));
            _resolverMock.Verify();
        }

        private class TestController : ApiController
        {
        }
    }
}
