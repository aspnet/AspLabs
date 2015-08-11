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
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Mocks;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookReceiverTests
    {
        private HttpConfiguration _config;
        private HttpRequestMessage _request;
        private HttpRequestContext _context;
        private WebHookReceiverMock _receiverMock;
        private Mock<IDependencyResolver> _resolverMock;

        public WebHookReceiverTests()
        {
            _config = new HttpConfiguration();
            _resolverMock = new Mock<IDependencyResolver>();
            _config.DependencyResolver = _resolverMock.Object;
            WebHooksConfig.Initialize(_config);
            _request = new HttpRequestMessage();
            _receiverMock = new WebHookReceiverMock();

            _context = new HttpRequestContext();
            _context.Configuration = _config;
            _request.SetRequestContext(_context);
        }

        public static TheoryData<byte[], byte[], bool> ByteCompareData
        {
            get
            {
                return new TheoryData<byte[], byte[], bool>
                {
                    { null, null, true },
                    { new byte[0], null, false },
                    { null, new byte[0], false },
                    { new byte[0], new byte[0], true },
                    { Encoding.UTF8.GetBytes("123"), Encoding.UTF8.GetBytes("1 2 3"), false },
                    { Encoding.UTF8.GetBytes("你好世界"), Encoding.UTF8.GetBytes("你好世界"), true },
                    { Encoding.UTF8.GetBytes("你好"), Encoding.UTF8.GetBytes("世界"), false },
                    { Encoding.UTF8.GetBytes(new string('a', 8 * 1024)), Encoding.UTF8.GetBytes(new string('a', 8 * 1024)), true },
                };
            }
        }

        [Theory]
        [InlineData("http://example.org")]
        [InlineData("HTTP://example.org")]
        [InlineData("http://local")]
        public async Task EnsureSecureConnection_ThrowsIfNotLocalAndNotHttps(string address)
        {
            // Arrange
            _request.RequestUri = new Uri(address);

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => _receiverMock.EnsureSecureConnection(_request));

            // Assert
            string expected = string.Format("The WebHook receiver 'WebHookReceiverMock' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.");
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expected, error.Message);
        }

        [Theory]
        [InlineData("12345678", 16, 32)]
        [InlineData("12345678", 2, 4)]
        [InlineData(null, 6, 8)]
        [InlineData("", 6, 8)]
        public async Task GetWebHookSecret_Throws_IfInvalidSecret(string secret, int minLength, int maxLength)
        {
            // Arrange
            string setting = "Secret";
            SettingsDictionary settings = _config.DependencyResolver.GetSettings();
            settings[setting] = secret;

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => _receiverMock.GetWebHookSecret(_request, setting, minLength, maxLength));

            // Assert
            string expected = string.Format("In order for the incoming WebHook request to be verified, the '{0}' setting must be set to a value between {1} and {2} characters long.", setting, minLength, maxLength);
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expected, error.Message);
        }

        [Theory]
        [InlineData("12345678", 2, 8)]
        [InlineData("12345678", 8, 16)]
        [InlineData("12345678", 2, 16)]
        [InlineData("", 0, 16)]
        public void GetWebHookSecret_Succeeds_IfValidSecret(string secret, int minLength, int maxLength)
        {
            // Arrange
            string setting = "Secret";
            SettingsDictionary settings = _config.DependencyResolver.GetSettings();
            settings[setting] = secret;

            // Act
            string actual = _receiverMock.GetWebHookSecret(_request, setting, minLength, maxLength);

            // Assert
            Assert.Equal(secret, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(10)]
        public async Task GetRequestHeader_Throws_IfHeaderCount_IsNotOne(int headers)
        {
            // Arrange
            string name = "signature";
            for (int cnt = 0; cnt < headers; cnt++)
            {
                _request.Headers.Add(name, "value");
            }

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => _receiverMock.GetRequestHeader(_request, name));

            // Assert
            string expected = string.Format("Expecting exactly one 'signature' header field in the WebHook request but found {0}. Please ensure that the request contains exactly one 'signature' header field.", headers);
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expected, error.Message);
        }

        [Fact]
        public void GetRequestHeader_Succeeds_WithOneHeader()
        {
            // Arrange
            string expected = "value";
            string name = "signature";
            _request.Headers.Add(name, expected);

            // Act
            string actual = _receiverMock.GetRequestHeader(_request, name);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReadAsJsonAsync_Throws_IfNullBody()
        {
            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsJsonAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request entity body cannot be empty.", error.Message);
        }

        [Fact]
        public async Task ReadAsJsonAsync_Throws_IfNotJson()
        {
            // Arrange
            _request.Content = new StringContent("Hello World", Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsJsonAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
        }

        [Fact]
        public async Task ReadAsJsonAsync_Throws_IfInvalidJson()
        {
            // Arrange
            _request.Content = new StringContent("I n v a l i d  J S O N", Encoding.UTF8, "application/json");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsJsonAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
        }

        [Fact]
        public async Task ReadAsJsonAsync_Succeeds_OnValidJson()
        {
            // Arrange
            _request.Content = new StringContent("{ \"k\": \"v\" }", Encoding.UTF8, "application/json");

            // Act
            JObject actual = await _receiverMock.ReadAsJsonAsync(_request);

            // Assert
            Assert.Equal("v", actual["k"]);
        }

        [Fact]
        public async Task ReadAsFormDataAsync_Throws_IfNullBody()
        {
            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsFormDataAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request entity body cannot be empty.", error.Message);
        }

        [Fact]
        public async Task ReadAsFormDataAsync_Throws_IfNotFormData()
        {
            // Arrange
            _request.Content = new StringContent("Hello World", Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsFormDataAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as HTML Form Data.", error.Message);
        }

        [Fact]
        public async Task ReadAsFormDataAsync_Succeeds_OnValidFormData()
        {
            // Arrange
            _request.Content = new StringContent("k=v", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            NameValueCollection actual = await _receiverMock.ReadAsFormDataAsync(_request);

            // Assert
            Assert.Equal("v", actual["k"]);
        }

        [Fact]
        public async Task CreateBadMethodResponse_CreatesExpectedResponse()
        {
            // Act
            HttpResponseMessage actual = _receiverMock.CreateBadMethodResponse(_request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP 'GET' method is not supported by the 'WebHookReceiverMock' WebHook receiver.", error.Message);
        }

        [Fact]
        public async Task CreateBadSignatureResponse_CreatesExpectedResponse()
        {
            // Act
            HttpResponseMessage actual = _receiverMock.CreateBadSignatureResponse(_request, "Header");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'Header' header field does not match the value expected by the 'WebHookReceiverMock' receiver. WebHook request is invalid.", error.Message);
        }

        [Fact]
        public async Task ExecuteWebHookAsync_StopsOnFirstResponse()
        {
            // Arrange
            List<TestHandler> handlers = new List<TestHandler>()
            {
                new TestHandler { Order = 10, },
                new TestHandler { Order = 20, },
                new TestHandler { Order = 30, SetResponse = true },
                new TestHandler { Order = 40, },
            };
            _resolverMock.Setup(r => r.GetServices(typeof(IWebHookHandler)))
                .Returns(handlers)
                .Verifiable();
            WebHookReceiverMock receiver = new WebHookReceiverMock();
            object data = new object();

            // Act
            HttpResponseMessage actual = await receiver.ExecuteWebHookAsync(WebHookReceiverMock.ReceiverName, _context, _request, new[] { "action" }, data);

            // Assert
            _resolverMock.Verify();
            Assert.Equal("Order: 30", actual.ReasonPhrase);
            Assert.True(handlers[0].IsCalled);
            Assert.True(handlers[1].IsCalled);
            Assert.True(handlers[2].IsCalled);
            Assert.False(handlers[3].IsCalled);
        }

        [Fact]
        public async Task ExecuteWebHookAsync_FindsMatchingHandlers()
        {
            // Arrange
            List<TestHandler> handlers = new List<TestHandler>()
            {
                new TestHandler { Order = 10, Receiver = "other" },
                new TestHandler { Order = 20, Receiver = "MockReceiver" },
                new TestHandler { Order = 30, Receiver = "MOCKRECEIVER" },
                new TestHandler { Order = 40, Receiver = null },
                new TestHandler { Order = 50, Receiver = "something" },
            };
            _resolverMock.Setup(r => r.GetServices(typeof(IWebHookHandler)))
                .Returns(handlers)
                .Verifiable();
            WebHookReceiverMock receiver = new WebHookReceiverMock();
            object data = new object();

            // Act
            HttpResponseMessage actual = await receiver.ExecuteWebHookAsync(WebHookReceiverMock.ReceiverName, _context, _request, new[] { "action" }, data);

            // Assert
            _resolverMock.Verify();
            Assert.Equal("OK", actual.ReasonPhrase);
            Assert.False(handlers[0].IsCalled);
            Assert.True(handlers[1].IsCalled);
            Assert.True(handlers[2].IsCalled);
            Assert.True(handlers[3].IsCalled);
            Assert.False(handlers[4].IsCalled);
        }

        [Theory]
        [MemberData("ByteCompareData")]
        public void SignatureEqual_ComparesCorrectly(byte[] inputA, byte[] inputB, bool expected)
        {
            // Act
            bool actual = WebHookReceiver.SignatureEqual(inputA, inputB);

            // Assert
            Assert.Equal(expected, actual);
        }

        private class TestHandler : IWebHookHandler
        {
            public int Order { get; set; }

            public string Receiver { get; set; }

            public bool SetResponse { get; set; }

            public bool IsCalled { get; set; }

            public Task ExecuteAsync(string receiver, WebHookHandlerContext context)
            {
                IsCalled = true;
                if (SetResponse)
                {
                    context.Response = new HttpResponseMessage();
                    context.Response.ReasonPhrase = "Order: " + Order;
                }
                return Task.FromResult(true);
            }
        }
    }
}