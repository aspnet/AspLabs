// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class PaypalWebHookReceiverTests
    {
        private const string TestContent = "{ \"key\": \"value\", \"event_type\": \"action1\" }";
        private const string TestReceiver = "Test";

        private HttpConfiguration _config;
        private SettingsDictionary _settings;
        private HttpRequestContext _context;
        private Mock<PaypalWebHookReceiver> _receiverMock;
        private HttpRequestMessage _postRequest;

        public PaypalWebHookReceiverTests()
        {
            _settings = new SettingsDictionary();

            _config = HttpConfigurationMock.Create(new Dictionary<Type, object> { { typeof(SettingsDictionary), _settings } });
            _context = new HttpRequestContext { Configuration = _config };

            _receiverMock = new Mock<PaypalWebHookReceiver>(false) { CallBase = true };

            _postRequest = new HttpRequestMessage() { Method = HttpMethod.Post };
            _postRequest.SetRequestContext(_context);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }

        [Fact]
        public void Constructor_Throws_IfNoConfig()
        {
            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => new PaypalWebHookReceiver());

            // Assert
            Assert.StartsWith("Initialization of the Paypal SDK failed:", ex.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfNoRequestBody()
        {
            // Arrange
            _receiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", _context, ItExpr.IsAny<NameValueCollection>(), string.Empty)
                .Returns(true)
                .Verifiable();
            _postRequest.Content = null;

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(string.Empty, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request entity body cannot be empty.", error.Message);
            _receiverMock.Verify();
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            _receiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", _context, ItExpr.IsAny<NameValueCollection>(), TestContent)
                .Returns(true)
                .Verifiable();
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            _receiverMock.Verify();
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoEventParameter()
        {
            // Arrange
            _receiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", _context, ItExpr.IsAny<NameValueCollection>(), "{ }")
                .Returns(true)
                .Verifiable();
            _postRequest.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'event_type' property.", error.Message);
            _receiverMock.Verify();
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfValidationFails()
        {
            // Arrange
            _receiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", _context, ItExpr.IsAny<NameValueCollection>(), TestContent)
                .Returns(false)
                .Verifiable();

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The received WebHook is not valid.", error.Message);
            _receiverMock.Verify();
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            List<string> actions = new List<string> { "action1" };
            _receiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", _context, ItExpr.IsAny<NameValueCollection>(), TestContent)
                .Returns(true)
                .Verifiable();
            _receiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", TestReceiver, _context, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
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
