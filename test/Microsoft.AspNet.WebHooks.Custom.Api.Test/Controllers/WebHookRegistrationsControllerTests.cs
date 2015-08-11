// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Controllers
{
    public class WebHookRegistrationsControllerTests
    {
        private const string Address = "http://localhost";
        private const string TestUser = "TestUser";
        private const string OtherUser = "OtherUser";
        private const int WebHookCount = 8;

        private HttpConfiguration _config;
        private WebHookRegistrationsControllerMock _controller;
        private HttpControllerContext _controllerContext;
        private Mock<IWebHookManager> _managerMock;
        private Mock<IDependencyResolver> _resolverMock;
        private IWebHookStore _store;
        private IWebHookUser _user;
        private IWebHookFilterManager _filterManager;
        private Mock<IWebHookFilterProvider> _filterProviderMock;

        public WebHookRegistrationsControllerTests()
        {
            _resolverMock = new Mock<IDependencyResolver>();
            _managerMock = new Mock<IWebHookManager>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookManager)))
                .Returns(_managerMock.Object)
                .Verifiable();

            _store = new MemoryWebHookStore();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookStore)))
                .Returns(_store)
                .Verifiable();

            _user = new WebHookUser();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookUser)))
                .Returns(_user)
                .Verifiable();

            _filterProviderMock = new Mock<IWebHookFilterProvider>();
            _filterManager = new WebHookFilterManager(new[]
            {
                new WildcardWebHookFilterProvider(),
                _filterProviderMock.Object
            });
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookFilterManager)))
                .Returns(_filterManager)
                .Verifiable();

            _config = new HttpConfiguration();
            _config.DependencyResolver = _resolverMock.Object;

            ClaimsIdentity identity = new ClaimsIdentity();
            Claim claim = new Claim(ClaimTypes.Name, TestUser);
            identity.AddClaim(claim);
            IPrincipal principal = new ClaimsPrincipal(identity);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Address);
            request.SetConfiguration(_config);
            HttpRequestContext requestContext = new HttpRequestContext()
            {
                Configuration = _config,
                Principal = principal,
                Url = new UrlHelper(request),
            };

            _controllerContext = new HttpControllerContext()
            {
                Configuration = _config,
                Request = new HttpRequestMessage(),
                RequestContext = requestContext,
            };
            _controller = new WebHookRegistrationsControllerMock();
            _controller.Initialize(_controllerContext);
        }

        [Fact]
        public async Task Get_Returns_ExpectedWebHooks()
        {
            // Arrange
            await Initialize();

            // Act
            IEnumerable<WebHook> actual = await _controller.Get();

            // Assert
            Assert.Equal(8, actual.Count());
            foreach (WebHook webHook in actual)
            {
                Assert.Equal(TestUser, webHook.Description);
            }
        }

        [Fact]
        public async Task Lookup_ReturnsOk_IfFoundWebHook()
        {
            // Arrange
            await Initialize();

            // Act
            IHttpActionResult actual = await _controller.Lookup("0");

            // Assert
            Assert.IsType<OkNegotiatedContentResult<WebHook>>(actual);
            WebHook webHook = ((OkNegotiatedContentResult<WebHook>)actual).Content;
            Assert.Equal(TestUser, webHook.Description);
        }

        [Fact]
        public async Task Lookup_ReturnsNotFound_IfFoundWebHook()
        {
            // Arrange
            await Initialize();

            // Act
            IHttpActionResult actual = await _controller.Lookup("9999");

            // Assert
            Assert.IsType<NotFoundResult>(actual);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_IfNoRequestBody()
        {
            // Arrange
            await Initialize();

            // Act
            IHttpActionResult actual = await _controller.Post(webHook: null);

            // Assert
            Assert.IsType<BadRequestResult>(actual);
        }

        [Fact]
        public async Task VerifyFilter_SetsWildcard_IfNoFiltersProvided()
        {
            // Arrange
            WebHook webHook = new WebHook();

            // Act
            await _controller.VerifyFilters(webHook);

            // Assert
            Assert.Equal("*", webHook.Filters.Single());
        }

        [Fact]
        public async Task VerifyFilter_Adds_NormalizedFilters()
        {
            // Arrange
            WebHook webHook = new WebHook();
            webHook.Filters.Add("FILTER");
            Collection<WebHookFilter> filters = new Collection<WebHookFilter>
            {
                new WebHookFilter { Name = "filter" }
            };
            _filterProviderMock.Setup(p => p.GetFiltersAsync())
                .ReturnsAsync(filters)
                .Verifiable();

            // Act
            await _controller.VerifyFilters(webHook);

            // Assert
            Assert.Equal("filter", webHook.Filters.Single());
        }

        private static WebHook CreateWebHook(string user, int offset, string filter = "a1")
        {
            WebHook hook = new WebHook
            {
                Id = offset.ToString(),
                Description = user,
                Secret = "123456789012345678901234567890123456789012345678",
                WebHookUri = "http://localhost/hook/" + offset
            };
            hook.Headers.Add("h1", "hv1");
            hook.Properties.Add("p1", "pv1");
            hook.Filters.Add(filter);
            return hook;
        }

        private async Task Initialize()
        {
            // Reset items for test user
            await _store.DeleteAllWebHooksAsync(TestUser);
            for (int cnt = 0; cnt < WebHookCount; cnt++)
            {
                WebHook webHook = CreateWebHook(TestUser, cnt);
                await _store.InsertWebHookAsync(TestUser, webHook);
            }

            // Insert items for other user which should not show up
            await _store.DeleteAllWebHooksAsync(OtherUser);
            for (int cnt = 0; cnt < WebHookCount; cnt++)
            {
                WebHook webHook = CreateWebHook(OtherUser, cnt);
                await _store.InsertWebHookAsync(OtherUser, webHook);
            }
        }

        private class WebHookRegistrationsControllerMock : WebHookRegistrationsController
        {
            public new void Initialize(HttpControllerContext controllerContext)
            {
                base.Initialize(controllerContext);
            }

            public new Task VerifyFilters(WebHook webHook)
            {
                return base.VerifyFilters(webHook);
            }
        }
    }
}
