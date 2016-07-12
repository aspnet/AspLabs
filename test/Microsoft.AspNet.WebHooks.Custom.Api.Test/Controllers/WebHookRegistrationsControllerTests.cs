// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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
        private Mock<IWebHookRegistrar> _registrarMock;
        private Mock<IWebHookIdValidator> _idValidatorMock;

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

            _registrarMock = new Mock<IWebHookRegistrar>();
            _idValidatorMock = new Mock<IWebHookIdValidator>();

            var services = new Dictionary<Type, object>
            {
                { typeof(IWebHookManager), _managerMock.Object },
                { typeof(IWebHookStore), _storeMock.Object },
                { typeof(IWebHookUser), _userMock.Object },
                { typeof(IWebHookFilterManager), _filterManager },
                { typeof(IWebHookRegistrar), _registrarMock.Object },
                { typeof(IWebHookIdValidator), _idValidatorMock.Object },
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

        public static TheoryData<string, string> NormalizedFilterData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { string.Empty, string.Empty },
                    { "FILTER", "filter" },
                    { "FiLTeR", "filter" },
                    { "Filter", "filter" },
                    { "filter", "Filter" },
                    { "你好世界", "你好世界" },
                };
            }
        }

        public static TheoryData<int, IEnumerable<string>, IEnumerable<string>> PrivateFilterData
        {
            get
            {
                string[] empty = new string[0];
                return new TheoryData<int, IEnumerable<string>, IEnumerable<string>>
                {
                    { 0, null, null },
                    { 4, new[] { "你", "好", "世", "界" }, new[] { "你", "好", "世", "界" } },
                    { 4, new[] { "MS_Private_" }, empty },

                    { 4, new[] { "ms_private_abc" }, empty },
                    { 4, new[] { "MS_Private_abc" }, empty },
                    { 4, new[] { "MS_PRIVATE_abc" }, empty },
                    { 4, new[] { "MS_PRIVATE_ABC" }, empty },

                    { 4, new[] { "a", "ms_private_abc" }, new[] { "a" } },
                    { 4, new[] { "a", "MS_Private_abc" }, new[] { "a" } },
                    { 4, new[] { "a", "MS_PRIVATE_abc" }, new[] { "a" } },
                    { 4, new[] { "a", "MS_PRIVATE_ABC" }, new[] { "a" } },

                    { 4, new[] { "ms_private_abc", "a" }, new[] { "a" } },
                    { 4, new[] { "MS_Private_abc", "a" }, new[] { "a" } },
                    { 4, new[] { "MS_PRIVATE_abc", "a" }, new[] { "a" } },
                    { 4, new[] { "MS_PRIVATE_ABC", "a" }, new[] { "a" } },
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
        public async Task Get_RemovesPrivateFilters()
        {
            // Arrange
            await Initialize(addPrivateFilter: true);

            // Act
            IEnumerable<WebHook> actual = await _controller.Get();

            // Assert
            Assert.Equal(8, actual.Count());
            foreach (WebHook webHook in actual)
            {
                Assert.False(webHook.Filters.Where(f => f.StartsWith(WebHookRegistrar.PrivateFilterPrefix)).Any());
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
        public async Task Lookup_RemovesPrivateFilters()
        {
            // Arrange
            await Initialize(addPrivateFilter: true);

            // Act
            IHttpActionResult actual = await _controller.Lookup("0");

            // Assert
            Assert.IsType<OkNegotiatedContentResult<WebHook>>(actual);
            WebHook webHook = ((OkNegotiatedContentResult<WebHook>)actual).Content;
            Assert.False(webHook.Filters.Where(f => f.StartsWith(WebHookRegistrar.PrivateFilterPrefix)).Any());
        }

        [Fact]
        public async Task Lookup_ReturnsNotFound_IfNotFoundWebHook()
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
        [MemberData(nameof(StatusData))]
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
        public async Task Post_Calls_IdValidator()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook();

            // Act
            IHttpActionResult actual = await _controller.Post(webHook);

            // Assert
            _idValidatorMock.Verify(v => v.ValidateIdAsync(_controllerContext.Request, webHook), Times.Once());
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
        [MemberData(nameof(StatusData))]
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

        [Theory]
        [MemberData(nameof(NormalizedFilterData))]
        public async Task VerifyFilter_Adds_NormalizedFilters(string input, string expected)
        {
            // Arrange
            WebHook webHook = new WebHook();
            webHook.Filters.Add(input);
            Collection<WebHookFilter> filters = new Collection<WebHookFilter>
            {
                new WebHookFilter { Name = expected }
            };
            _filterProviderMock.Setup(p => p.GetFiltersAsync())
                .ReturnsAsync(filters)
                .Verifiable();

            // Act
            await _controller.VerifyFilters(webHook);

            // Assert
            _filterProviderMock.Verify();
            Assert.Equal(expected, webHook.Filters.Single());
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
            _filterProviderMock.Verify();
        }

        [Fact]
        public async Task VerifyFilter_Calls_RegistrarWithNoFilter()
        {
            // Arrange
            WebHook webHook = new WebHook();
            _registrarMock.Setup(r => r.RegisterAsync(_controllerContext.Request, webHook))
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Act
            await _controller.VerifyFilters(webHook);

            // Assert
            _registrarMock.Verify();
        }

        [Fact]
        public async Task VerifyFilter_Calls_RegistrarWithFilter()
        {
            // Arrange
            WebHook webHook = new WebHook();
            webHook.Filters.Add(FilterName);
            _registrarMock.Setup(r => r.RegisterAsync(_controllerContext.Request, webHook))
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Act
            await _controller.VerifyFilters(webHook);

            // Assert
            _registrarMock.Verify();
        }

        [Fact]
        public async Task VerifyFilter_Throws_IfRegistrarThrows()
        {
            // Arrange
            Exception ex = new Exception("Catch this!");
            WebHook webHook = new WebHook();
            webHook.Filters.Add(FilterName);
            _registrarMock.Setup(r => r.RegisterAsync(_controllerContext.Request, webHook))
                .Throws(ex);

            // Act
            HttpResponseException rex = await Assert.ThrowsAsync<HttpResponseException>(() => _controller.VerifyFilters(webHook));

            // Assert
            HttpError error = await rex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'IWebHookRegistrarProxy' implementation of 'IWebHookRegistrar' caused an exception: Catch this!", error.Message);
        }

        [Fact]
        public async Task VerifyFilter_Throws_HttpException_IfRegistrarThrows()
        {
            // Arrange
            HttpResponseException rex = new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Conflict));
            WebHook webHook = new WebHook();
            webHook.Filters.Add(FilterName);
            _registrarMock.Setup(r => r.RegisterAsync(_controllerContext.Request, webHook))
                .Throws(rex);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _controller.VerifyFilters(webHook));

            // Assert
            Assert.Same(rex, ex);
        }

        [Theory]
        [MemberData(nameof(PrivateFilterData))]
        public void RemovePrivateFilters_Succeeds(int count, string[] input, string[] expected)
        {
            // Arrange
            List<WebHook> webHooks = new List<WebHook>();
            for (int cnt = 0; cnt < count; cnt++)
            {
                WebHook webHook = new WebHook();
                foreach (string i in input)
                {
                    webHook.Filters.Add(i);
                }
                webHooks.Add(webHook);
            }

            // Act
            _controller.RemovePrivateFilters(webHooks);

            // Assert
            for (int cnt = 0; cnt < count; cnt++)
            {
                Assert.Equal(expected, webHooks[cnt].Filters);
            }
        }

        private static WebHook CreateWebHook(string user, int offset, string filter = "a1", bool addPrivateFilter = false)
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
            if (addPrivateFilter)
            {
                string privateFilter = WebHookRegistrar.PrivateFilterPrefix + "abc";
                hook.Filters.Add(privateFilter);
            }
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

        private async Task Initialize(bool addPrivateFilter = false)
        {
            // Reset items for test user
            await _storeMock.Object.DeleteAllWebHooksAsync(TestUser);
            for (int cnt = 0; cnt < WebHookCount; cnt++)
            {
                WebHook webHook = CreateWebHook(TestUser, cnt, addPrivateFilter: addPrivateFilter);
                await _storeMock.Object.InsertWebHookAsync(TestUser, webHook);
            }

            // Insert items for other user which should not show up
            await _storeMock.Object.DeleteAllWebHooksAsync(OtherUser);
            for (int cnt = 0; cnt < WebHookCount; cnt++)
            {
                WebHook webHook = CreateWebHook(OtherUser, cnt, addPrivateFilter: addPrivateFilter);
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

            public new void RemovePrivateFilters(IEnumerable<WebHook> webHooks)
            {
                base.RemovePrivateFilters(webHooks);
            }
        }
    }
}
