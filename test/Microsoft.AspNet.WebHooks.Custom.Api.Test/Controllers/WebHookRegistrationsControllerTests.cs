// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Microsoft.AspNet.WebHooks.Routes;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Controllers
{
    public class WebHookRegistrationsControllerTests
    {
        private const string Address = "http://localhost";
        private const string TestUser = "TestUser";
        private const string OtherUser = "OtherUser";
        private const string FilterName = "Filter";
        private const string TestWebHookId = "12345";
        private const int WebHookCount = 8;

        private HttpConfiguration _config;
        private WebHookRegistrationsControllerMock _controller;
        private HttpControllerContext _controllerContext;
        private Mock<IWebHookManager> _managerMock;
        private Mock<MemoryWebHookStore> _storeMock;
        private Mock<IWebHookUser> _userMock;
        private IWebHookFilterManager _filterManager;
        private Mock<IWebHookFilterProvider> _filterProviderMock;

        public WebHookRegistrationsControllerTests()
        {
            IPrincipal principal = new ClaimsPrincipal();

            _managerMock = new Mock<IWebHookManager>();
            _storeMock = new Mock<MemoryWebHookStore> { CallBase = true };

            _userMock = new Mock<IWebHookUser>();
            _userMock.Setup(u => u.GetUserIdAsync(principal))
                .ReturnsAsync(TestUser);

            _filterProviderMock = new Mock<IWebHookFilterProvider>();
            _filterProviderMock.Setup(p => p.GetFiltersAsync())
                .ReturnsAsync(new Collection<WebHookFilter> { new WebHookFilter { Name = FilterName } });

            _filterManager = new WebHookFilterManager(new[]
            {
                new WildcardWebHookFilterProvider(),
                _filterProviderMock.Object
            });

            var services = new Dictionary<Type, object>
            {
                { typeof(IWebHookManager), _managerMock.Object },
                { typeof(IWebHookStore), _storeMock.Object },
                { typeof(IWebHookUser), _userMock.Object },
                { typeof(IWebHookFilterManager), _filterManager }
            };
            _config = HttpConfigurationMock.Create(services);
            _config.Routes.Add(WebHookRouteNames.FiltersGetAction, new HttpRoute());

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

        public static TheoryData<StoreResult, Type> StatusData
        {
            get
            {
                return new TheoryData<StoreResult, Type>
                {
                    { StoreResult.Conflict, typeof(ConflictResult) },
                    { StoreResult.NotFound, typeof(NotFoundResult) },
                    { StoreResult.OperationError, typeof(BadRequestResult) },
                };
            }
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
        public async Task Post_ReturnsBadRequest_IfInvalidFilter()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook(filterName: "unknown");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _controller.Post(webHook));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The following filters are not valid: 'unknown'. A list of valid filters can be obtained from the path 'http://localhost/'.", error.Message);
        }

        [Fact]
        public async Task Post_ReturnsInternalServerError_IfStoreThrows()
        {
            // Arrange
            WebHook webHook = CreateWebHook();
            _storeMock.Setup(s => s.InsertWebHookAsync(TestUser, webHook))
                .Throws<Exception>();

            // Act
            IHttpActionResult actual = await _controller.Post(webHook);

            // Assert
            Assert.IsType<ResponseMessageResult>(actual);
            ResponseMessageResult result = actual as ResponseMessageResult;
            HttpError error = await result.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Could not register WebHook due to error: Exception of type 'System.Exception' was thrown.", error.Message);
        }

        [Theory]
        [MemberData("StatusData")]
        public async Task Post_ReturnsError_IfStoreReturnsNonsuccess(StoreResult result, Type response)
        {
            // Arrange
            WebHook webHook = CreateWebHook();
            _storeMock.Setup(s => s.InsertWebHookAsync(TestUser, webHook))
                .ReturnsAsync(result);

            // Act
            IHttpActionResult actual = await _controller.Post(webHook);

            // Assert
            Assert.IsType(response, actual);
        }

        [Fact]
        public async Task Post_ReturnsCreated_IfValidWebHook()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook();

            // Act
            IHttpActionResult actual = await _controller.Post(webHook);

            // Assert
            Assert.IsType<CreatedAtRouteNegotiatedContentResult<WebHook>>(actual);
        }

        [Fact]
        public async Task Put_ReturnsBadRequest_IfNoRequestBody()
        {
            // Arrange
            await Initialize();

            // Act
            IHttpActionResult actual = await _controller.Put(TestWebHookId, webHook: null);

            // Assert
            Assert.IsType<BadRequestResult>(actual);
        }

        [Fact]
        public async Task Put_ReturnsBadRequest_IfInvalidFilter()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook(filterName: "unknown");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _controller.Post(webHook));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The following filters are not valid: 'unknown'. A list of valid filters can be obtained from the path 'http://localhost/'.", error.Message);
        }

        [Fact]
        public async Task Put_ReturnsBadRequest_IfWebHookIdDiffersFromUriId()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook();

            // Act
            IHttpActionResult actual = await _controller.Put("unknown", webHook);

            // Assert
            Assert.IsType<BadRequestResult>(actual);
        }

        [Fact]
        public async Task Put_ReturnsInternalServerError_IfStoreThrows()
        {
            // Arrange
            WebHook webHook = CreateWebHook();
            _storeMock.Setup(s => s.UpdateWebHookAsync(TestUser, webHook))
                .Throws<Exception>();

            // Act
            IHttpActionResult actual = await _controller.Put(TestWebHookId, webHook);

            // Assert
            Assert.IsType<ResponseMessageResult>(actual);
            ResponseMessageResult result = actual as ResponseMessageResult;
            HttpError error = await result.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Could not update WebHook due to error: Exception of type 'System.Exception' was thrown.", error.Message);
        }

        [Theory]
        [MemberData("StatusData")]
        public async Task Put_ReturnsError_IfStoreReturnsNonsuccess(StoreResult result, Type response)
        {
            // Arrange
            WebHook webHook = CreateWebHook();
            _storeMock.Setup(s => s.UpdateWebHookAsync(TestUser, webHook))
                .ReturnsAsync(result);

            // Act
            IHttpActionResult actual = await _controller.Put(TestWebHookId, webHook);

            // Assert
            Assert.IsType(response, actual);
        }

        [Fact]
        public async Task Put_ReturnsOk_IfValidWebHook()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook();
            await _controller.Post(webHook);

            // Act
            IHttpActionResult actual = await _controller.Put(webHook.Id, webHook);

            // Assert
            Assert.IsType<OkResult>(actual);
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

        [Fact]
        public async Task VerifyFilter_Throws_IfInvalidFilters()
        {
            // Arrange
            WebHook webHook = new WebHook();
            webHook.Filters.Add("Unknown");
            Collection<WebHookFilter> filters = new Collection<WebHookFilter>
            {
            };
            _filterProviderMock.Setup(p => p.GetFiltersAsync())
                .ReturnsAsync(filters)
                .Verifiable();

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _controller.VerifyFilters(webHook));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The following filters are not valid: 'Unknown'. A list of valid filters can be obtained from the path 'http://localhost/'.", error.Message);
        }

        private static WebHook CreateWebHook(string user, int offset, string filter = "a1")
        {
            WebHook hook = new WebHook
            {
                Id = offset.ToString(),
                Description = user,
                Secret = "123456789012345678901234567890123456789012345678",
                WebHookUri = new Uri("http://localhost/hook/" + offset),
            };
            hook.Headers.Add("h1", "hv1");
            hook.Properties.Add("p1", "pv1");
            hook.Filters.Add(filter);
            return hook;
        }

        private static WebHook CreateWebHook(string filterName = FilterName)
        {
            WebHook webHook = new WebHook()
            {
                Id = TestWebHookId,
                WebHookUri = new Uri(Address)
            };
            webHook.Filters.Add(filterName);
            return webHook;
        }

        private async Task Initialize()
        {
            // Reset items for test user
            await _storeMock.Object.DeleteAllWebHooksAsync(TestUser);
            for (int cnt = 0; cnt < WebHookCount; cnt++)
            {
                WebHook webHook = CreateWebHook(TestUser, cnt);
                await _storeMock.Object.InsertWebHookAsync(TestUser, webHook);
            }

            // Insert items for other user which should not show up
            await _storeMock.Object.DeleteAllWebHooksAsync(OtherUser);
            for (int cnt = 0; cnt < WebHookCount; cnt++)
            {
                WebHook webHook = CreateWebHook(OtherUser, cnt);
                await _storeMock.Object.InsertWebHookAsync(OtherUser, webHook);
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
