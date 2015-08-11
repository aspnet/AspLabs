// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public class MailChimpWebHookReceiverTests
    {
        private const string TestReceiver = "Test";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestHost = "https://some.ssl.host?code=" + TestSecret;

        private HttpConfiguration _config;
        private SettingsDictionary _settings;
        private HttpRequestContext _context;
        private Mock<MailChimpWebHookReceiver> _receiverMock;

        private HttpRequestMessage _postRequest;
        private HttpRequestMessage _getRequest;

        public MailChimpWebHookReceiverTests()
        {
            _settings = new SettingsDictionary();
            _settings["MS_WebHookReceiverSecret_MailChimp"] = TestSecret;

            _config = HttpConfigurationMock.Create(new Dictionary<Type, object> { { typeof(SettingsDictionary), _settings } });
            _context = new HttpRequestContext { Configuration = _config };

            _receiverMock = new Mock<MailChimpWebHookReceiver> { CallBase = true };

            _postRequest = new HttpRequestMessage(HttpMethod.Post, TestHost);
            _postRequest.SetRequestContext(_context);

            _getRequest = new HttpRequestMessage(HttpMethod.Get, TestHost);
            _getRequest.SetRequestContext(_context);
        }

        public static TheoryData<string> InvalidCodeQueries
        {
            get
            {
                return new TheoryData<string>
                {
                    string.Empty,
                    "=",
                    "==",
                    "invalid",
                    "code",
                    "code=",
                    "k1=v1;k2=v2",
                };
            }
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotUsingHttps()
        {
            // Arrange
            _postRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'MailChimpWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("InvalidCodeQueries")]
        public async Task ReceiveAsync_Throws_IfPostHasNoCodeParameter(string query)
        {
            // Arrange
            _postRequest.RequestUri = new Uri("https://some.no.ssl.host?" + query);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook verification request must contain a 'code' query parameter.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasWrongCodeParameter()
        {
            // Arrange
            _postRequest.RequestUri = new Uri("https://some.no.ssl.host?code=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'code' query parameter provided in the HTTP request did not match the expected value.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotFormData()
        {
            // Arrange
            _postRequest.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as HTML Form Data.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoAction()
        {
            // Arrange
            _postRequest.Content = new StringContent("k=v", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'type' property.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            string content = "type=hello";
            List<string> actions = new List<string> { "hello" };
            _postRequest.Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");
            _receiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", TestReceiver, _context, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            _receiverMock.Verify();
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfGetIsNotUsingHttps()
        {
            // Arrange
            _getRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _getRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'MailChimpWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("InvalidCodeQueries")]
        public async Task ReceiveAsync_Throws_IfGetHasNoCodeParameter(string query)
        {
            // Arrange
            _getRequest.RequestUri = new Uri("https://some.no.ssl.host?" + query);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _getRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook verification request must contain a 'code' query parameter.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfGetHasWrongCodeParameter()
        {
            // Arrange
            _getRequest.RequestUri = new Uri("https://some.no.ssl.host?code=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _getRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'code' query parameter provided in the HTTP request did not match the expected value.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidGet()
        {
            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _getRequest);

            // Assert
            Assert.Equal(actual.StatusCode, HttpStatusCode.OK);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
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
