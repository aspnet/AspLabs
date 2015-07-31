// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class StripeWebHookReceiverTests
    {
        private const string TestReceiver = "Test";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestId = "12345";

        private HttpConfiguration _config;
        private SettingsDictionary _settings;
        private HttpRequestContext _context;

        private Mock<StripeWebHookReceiver> _receiverMock;
        private HttpClient _httpClient;
        private HttpMessageHandlerMock _handlerMock;

        private HttpRequestMessage _postRequest;
        private HttpResponseMessage _stripeResponse;

        public StripeWebHookReceiverTests()
        {
            _settings = new SettingsDictionary();
            _settings["MS_WebHookReceiverSecret_Stripe"] = TestSecret;

            _config = HttpConfigurationMock.Create(new Dictionary<Type, object> { { typeof(SettingsDictionary), _settings } });
            _context = new HttpRequestContext { Configuration = _config };

            _stripeResponse = new HttpResponseMessage();
            _stripeResponse.Content = new StringContent("{ \"type\": \"action\" }", Encoding.UTF8, "application/json");

            _handlerMock = new HttpMessageHandlerMock();
            _handlerMock.Handler = (req, counter) =>
            {
                string expected = string.Format(CultureInfo.InvariantCulture, StripeWebHookReceiver.EventUriTemplate, TestId);
                Assert.Equal(req.RequestUri.AbsoluteUri, expected);
                return Task.FromResult(_stripeResponse);
            };

            _httpClient = new HttpClient(_handlerMock);
            _receiverMock = new Mock<StripeWebHookReceiver>(_httpClient) { CallBase = true };

            _postRequest = new HttpRequestMessage { Method = HttpMethod.Post };
            _postRequest.SetRequestContext(_context);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            _postRequest.Content = new StringContent("k=v", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            Assert.Equal(0, _handlerMock.Counter);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoId()
        {
            // Arrange
            _postRequest.Content = new StringContent("{ \"type\": \"action\" }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'id' property.", error.Message);
            Assert.Equal(0, _handlerMock.Counter);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfTestId()
        {
            // Arrange
            _postRequest.Content = new StringContent("{ \"type\": \"action\", \"id\": \"" + StripeWebHookReceiver.TestId + "\" }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Equal(0, _handlerMock.Counter);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasUnknownId()
        {
            // Arrange
            _postRequest.Content = new StringContent("{ \"type\": \"action\", \"id\": \"" + TestId + "\" }", Encoding.UTF8, "application/json");
            _handlerMock.Handler = (req, counter) =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            };

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The ID '12345' could not be resolved for an actual event.", error.Message);
            Assert.Equal(1, _handlerMock.Counter);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            _postRequest.Content = new StringContent("{ \"type\": \"action\", \"id\": \"" + TestId + "\" }", Encoding.UTF8, "application/json");
            _receiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            Assert.Equal(1, _handlerMock.Counter);
            _receiverMock.Verify();
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        [InlineData("OPTIONS")]
        public async Task ReceiveAsync_ReturnsError_IfInvalidMethod(string method)
        {
            // Arrange
            HttpRequestMessage req = new HttpRequestMessage { Method = new HttpMethod(method) };
            req.SetRequestContext(_context);

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, req);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, req, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }
    }
}
