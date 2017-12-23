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
        private const string FilterName = "Filter";
        private const int WebHookCount = 8;

        private IPrincipal _principal;
        private HttpConfiguration _config;
        private HttpRequestMessage _request;
        private WebHookRegistrationsControllerMock _controller;
        private HttpControllerContext _controllerContext;

        private Mock<IWebHookRegistrationsManager> _regsMock;
        private Mock<IWebHookRegistrar> _registrarMock;
        private Mock<IWebHookIdValidator> _idValidator;

        public WebHookRegistrationsControllerTests()
        {
            _regsMock = new Mock<IWebHookRegistrationsManager>();
            _registrarMock = new Mock<IWebHookRegistrar>();
            _idValidator = new Mock<IWebHookIdValidator>();

            _principal = new ClaimsPrincipal();
            var services = new Dictionary<Type, object>
            {
                { typeof(IWebHookRegistrationsManager), _regsMock.Object },
                { typeof(IWebHookRegistrar), _registrarMock.Object },
                { typeof(IWebHookIdValidator), _idValidator.Object },
            };
            _config = HttpConfigurationMock.Create(services);
            _config.Routes.Add(WebHookRouteNames.FiltersGetAction, new HttpRoute());

            _request = new HttpRequestMessage(HttpMethod.Get, Address);
            _request.SetConfiguration(_config);
            var requestContext = new HttpRequestContext()
            {
                Configuration = _config,
                Principal = _principal,
                Url = new UrlHelper(_request),
            };

            _controllerContext = new HttpControllerContext()
            {
                Configuration = _config,
                Request = _request,
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

        public static TheoryData<IEnumerable<string>, IEnumerable<string>> PrivateFilterData
        {
            get
            {
                var empty = new string[0];
                return new TheoryData<IEnumerable<string>, IEnumerable<string>>
                {
                    { new[] { "你", "好", "世", "界" }, new[] { "你", "好", "世", "界" } },
                    { new[] { "MS_Private_" }, empty },

                    { new[] { "ms_private_abc" }, empty },
                    { new[] { "MS_Private_abc" }, empty },
                    { new[] { "MS_PRIVATE_abc" }, empty },
                    { new[] { "MS_PRIVATE_ABC" }, empty },

                    { new[] { "a", "ms_private_abc" }, new[] { "a" } },
                    { new[] { "a", "MS_Private_abc" }, new[] { "a" } },
                    { new[] { "a", "MS_PRIVATE_abc" }, new[] { "a" } },
                    { new[] { "a", "MS_PRIVATE_ABC" }, new[] { "a" } },

                    { new[] { "ms_private_abc", "a" }, new[] { "a" } },
                    { new[] { "MS_Private_abc", "a" }, new[] { "a" } },
                    { new[] { "MS_PRIVATE_abc", "a" }, new[] { "a" } },
                    { new[] { "MS_PRIVATE_ABC", "a" }, new[] { "a" } },
                };
            }
        }

        [Fact]
        public async Task Get_Returns_WebHooks()
        {
            // Arrange
            IEnumerable<WebHook> hooks = CreateWebHooks();
            _regsMock.Setup(r => r.GetWebHooksAsync(_principal, It.IsAny<Func<string, WebHook, Task>>()))
                .ReturnsAsync(hooks)
                .Verifiable();

            // Act
            var actual = await _controller.Get();

            // Assert
            _regsMock.Verify();
            Assert.Equal(WebHookCount, actual.Count());
        }

        [Fact]
        public async Task Get_Returns_EmptyList()
        {
            // Arrange
            _regsMock.Setup(r => r.GetWebHooksAsync(_principal, It.IsAny<Func<string, WebHook, Task>>()))
                .ReturnsAsync(new WebHook[0])
                .Verifiable();

            // Act
            var actual = await _controller.Get();

            // Assert
            _regsMock.Verify();
            Assert.Empty(actual);
        }

        [Fact]
        public async Task Lookup_Returns_WebHook()
        {
            // Arrange
            var hook = CreateWebHook();
            _regsMock.Setup(r => r.LookupWebHookAsync(_principal, TestUser, It.IsAny<Func<string, WebHook, Task>>()))
                .ReturnsAsync(hook)
                .Verifiable();

            // Act
            var result = await _controller.Lookup(TestUser);
            var actual = ((OkNegotiatedContentResult<WebHook>)result).Content;

            // Assert
            Assert.Equal(TestUser, actual.Id);
        }

        [Fact]
        public async Task Lookup_ReturnsNotFound_IfNotFoundWebHook()
        {
            // Arrange
            _regsMock.Setup(r => r.LookupWebHookAsync(_principal, TestUser, It.IsAny<Func<string, WebHook, Task>>()))
                .ReturnsAsync(null)
                .Verifiable();

            // Act
            var actual = await _controller.Lookup(TestUser);

            // Assert
            Assert.IsType<NotFoundResult>(actual);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_IfNoRequestBody()
        {
            // Act
            var actual = await _controller.Post(webHook: null);

            // Assert
            Assert.IsType<BadRequestResult>(actual);
        }

        [Theory]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public async Task Post_ReturnsBadRequest_IfValidationFails(bool failId, bool failSecret, bool failFilters, bool failAddress)
        {
            // Arrange
            var webHook = CreateWebHook();
            if (failId)
            {
                _idValidator.Setup(v => v.ValidateIdAsync(_request, webHook))
                    .Throws<Exception>();
            }
            if (failSecret)
            {
                _regsMock.Setup(v => v.VerifySecretAsync(webHook))
                    .Throws<Exception>();
            }
            if (failFilters)
            {
                _regsMock.Setup(v => v.VerifyFiltersAsync(webHook))
                    .Throws<Exception>();
            }
            if (failAddress)
            {
                _regsMock.Setup(v => v.VerifyAddressAsync(webHook))
                    .Throws<Exception>();
            }

            // Act
            var actual = await _controller.Post(webHook);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, ((ResponseMessageResult)actual).Response.StatusCode);
        }

        [Fact]
        public async Task Post_ReturnsInternalServerError_IfStoreThrows()
        {
            // Arrange
            var webHook = CreateWebHook();
            _regsMock.Setup(s => s.AddWebHookAsync(_principal, webHook, It.IsAny<Func<string, WebHook, Task>>()))
                .Throws<Exception>();

            // Act
            var actual = await _controller.Post(webHook);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, ((ResponseMessageResult)actual).Response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(StatusData))]
        public async Task Post_ReturnsError_IfStoreReturnsNonsuccess(StoreResult result, Type response)
        {
            // Arrange
            var webHook = CreateWebHook();
            _regsMock.Setup(s => s.AddWebHookAsync(_principal, webHook, It.IsAny<Func<string, WebHook, Task>>()))
                .ReturnsAsync(result);

            // Act
            var actual = await _controller.Post(webHook);

            // Assert
            Assert.IsType(response, actual);
        }

        [Fact]
        public async Task Post_ReturnsCreated_IfValidWebHook()
        {
            // Arrange
            var webHook = CreateWebHook();
            _regsMock.Setup(s => s.AddWebHookAsync(_principal, webHook, It.IsAny<Func<string, WebHook, Task>>()))
                .ReturnsAsync(StoreResult.Success);

            // Act
            var actual = await _controller.Post(webHook);

            // Assert
            Assert.IsType<CreatedAtRouteNegotiatedContentResult<WebHook>>(actual);
        }

        [Fact]
        public async Task Put_ReturnsBadRequest_IfNoRequestBody()
        {
            // Act
            var actual = await _controller.Put(TestUser, webHook: null);

            // Assert
            Assert.IsType<BadRequestResult>(actual);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public async Task Put_ReturnsBadRequest_IfValidationFails(bool failSecret, bool failFilters, bool failAddress)
        {
            // Arrange
            var webHook = CreateWebHook();
            if (failSecret)
            {
                _regsMock.Setup(v => v.VerifySecretAsync(webHook))
                    .Throws<Exception>();
            }
            if (failFilters)
            {
                _regsMock.Setup(v => v.VerifyFiltersAsync(webHook))
                    .Throws<Exception>();
            }
            if (failAddress)
            {
                _regsMock.Setup(v => v.VerifyAddressAsync(webHook))
                    .Throws<Exception>();
            }

            // Act
            var actual = await _controller.Put(TestUser, webHook);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, ((ResponseMessageResult)actual).Response.StatusCode);
        }

        [Fact]
        public async Task Put_ReturnsBadRequest_IfWebHookIdDiffersFromUriId()
        {
            // Arrange
            var webHook = CreateWebHook();

            // Act
            var actual = await _controller.Put("unknown", webHook);

            // Assert
            Assert.IsType<BadRequestResult>(actual);
        }

        [Fact]
        public async Task Put_ReturnsInternalServerError_IfStoreThrows()
        {
            // Arrange
            var webHook = CreateWebHook();
            _regsMock.Setup(s => s.UpdateWebHookAsync(_principal, webHook, It.IsAny<Func<string, WebHook, Task>>()))
                .Throws<Exception>();

            // Act
            var actual = await _controller.Put(TestUser, webHook);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, ((ResponseMessageResult)actual).Response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(StatusData))]
        public async Task Put_ReturnsError_IfStoreReturnsNonsuccess(StoreResult result, Type response)
        {
            // Arrange
            var webHook = CreateWebHook();
            _regsMock.Setup(s => s.UpdateWebHookAsync(_principal, webHook, It.IsAny<Func<string, WebHook, Task>>()))
                .ReturnsAsync(result);

            // Act
            var actual = await _controller.Put(TestUser, webHook);

            // Assert
            Assert.IsType(response, actual);
        }

        [Fact]
        public async Task Put_ReturnsOk_IfValidWebHook()
        {
            // Arrange
            var webHook = CreateWebHook();
            _regsMock.Setup(s => s.UpdateWebHookAsync(_principal, webHook, null))
                .ReturnsAsync(StoreResult.Success);

            // Act
            var actual = await _controller.Put(webHook.Id, webHook);

            // Assert
            Assert.IsType<OkResult>(actual);
        }

        [Fact]
        public async Task AddPrivateFilters_Calls_RegistrarWithNoFilter()
        {
            // Arrange
            var webHook = new WebHook();
            _registrarMock.Setup(r => r.RegisterAsync(_controllerContext.Request, webHook))
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Act
            await _controller.AddPrivateFilters("12345", webHook);

            // Assert
            _registrarMock.Verify();
        }

        [Fact]
        public async Task AddPrivateFilters_Calls_RegistrarWithFilter()
        {
            // Arrange
            var webHook = new WebHook();
            webHook.Filters.Add(FilterName);
            _registrarMock.Setup(r => r.RegisterAsync(_controllerContext.Request, webHook))
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Act
            await _controller.AddPrivateFilters("12345", webHook);

            // Assert
            _registrarMock.Verify();
        }

        [Fact]
        public async Task AddPrivateFilters_Throws_IfRegistrarThrows()
        {
            // Arrange
            var ex = new Exception("Catch this!");
            var webHook = new WebHook();
            webHook.Filters.Add(FilterName);
            _registrarMock.Setup(r => r.RegisterAsync(_controllerContext.Request, webHook))
                .Throws(ex);

            // Act
            var rex = await Assert.ThrowsAsync<HttpResponseException>(() => _controller.AddPrivateFilters("12345", webHook));

            // Assert
            var error = await rex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'IWebHookRegistrarProxy' implementation of 'IWebHookRegistrar' caused an exception: Catch this!", error.Message);
        }

        [Fact]
        public async Task AddPrivateFilters_Throws_HttpException_IfRegistrarThrows()
        {
            // Arrange
            var rex = new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Conflict));
            var webHook = new WebHook();
            webHook.Filters.Add(FilterName);
            _registrarMock.Setup(r => r.RegisterAsync(_controllerContext.Request, webHook))
                .Throws(rex);

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => _controller.AddPrivateFilters("12345", webHook));

            // Assert
            Assert.Same(rex, ex);
        }

        [Theory]
        [MemberData(nameof(PrivateFilterData))]
        public void RemovePrivateFilters_Succeeds(string[] input, string[] expected)
        {
            // Arrange
            var webHook = new WebHook();
            foreach (var i in input)
            {
                webHook.Filters.Add(i);
            }

            // Act
            _controller.RemovePrivateFilters(TestUser, webHook);

            // Assert
            Assert.Equal(expected, webHook.Filters);
        }

        private static WebHook CreateWebHook(string id = TestUser, string filterName = FilterName, bool addPrivateFilter = false)
        {
            var webHook = new WebHook()
            {
                Id = id,
                WebHookUri = new Uri(Address)
            };

            webHook.Filters.Add(filterName);
            if (addPrivateFilter)
            {
                var privateFilter = WebHookRegistrar.PrivateFilterPrefix + "abc";
                webHook.Filters.Add(privateFilter);
            }

            return webHook;
        }

        private Collection<WebHook> CreateWebHooks(bool addPrivateFilter = false)
        {
            var hooks = new Collection<WebHook>();
            for (var i = 0; i < WebHookCount; i++)
            {
                var webHook = CreateWebHook(id: i.ToString(), filterName: "a" + i.ToString(), addPrivateFilter: addPrivateFilter);
                hooks.Add(webHook);
            }
            return hooks;
        }

        private class WebHookRegistrationsControllerMock : WebHookRegistrationsController
        {
            public new void Initialize(HttpControllerContext controllerContext)
            {
                base.Initialize(controllerContext);
            }

            public new Task RemovePrivateFilters(string user, WebHook webHook)
            {
                return base.RemovePrivateFilters(user, webHook);
            }

            public new Task AddPrivateFilters(string user, WebHook webHook)
            {
                return base.AddPrivateFilters(user, webHook);
            }
        }
    }
}
