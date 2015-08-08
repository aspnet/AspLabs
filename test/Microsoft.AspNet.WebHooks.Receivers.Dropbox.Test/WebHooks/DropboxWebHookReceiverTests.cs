// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
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
    public class DropboxWebHookReceiverTests
    {
        private const string TestContent = "{ \"key\": \"value\" }";
        private const string TestReceiver = "Test";
        private const string TestSecret = "12345678901234567890123456789012";

        private HttpConfiguration _config;
        private SettingsDictionary _settings;
        private HttpRequestContext _context;
        private Mock<DropboxWebHookReceiver> _receiverMock;

        private HttpRequestMessage _getRequest;
        private HttpRequestMessage _postRequest;

        private string _testSignature;

        public DropboxWebHookReceiverTests()
        {
            _settings = new SettingsDictionary();
            _settings["MS_WebHookReceiverSecret_Dropbox"] = TestSecret;

            _config = HttpConfigurationMock.Create(new Dictionary<Type, object> { { typeof(SettingsDictionary), _settings } });
            _context = new HttpRequestContext { Configuration = _config };

            _receiverMock = new Mock<DropboxWebHookReceiver> { CallBase = true };

            _getRequest = new HttpRequestMessage();
            _getRequest.SetRequestContext(_context);

            _postRequest = new HttpRequestMessage() { Method = HttpMethod.Post };
            _postRequest.SetRequestContext(_context);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");

            byte[] secret = Encoding.UTF8.GetBytes(TestSecret);
            using (var hasher = new HMACSHA256(secret))
            {
                byte[] data = Encoding.UTF8.GetBytes(TestContent);
                byte[] testHash = hasher.ComputeHash(data);
                _testSignature = EncodingUtilities.ToHex(testHash);
            }
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasNoSignatureHeader()
        {
            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Dropbox-Signature' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'X-Dropbox-Signature' header field.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            _postRequest.Headers.Add(DropboxWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(DropboxWebHookReceiver.SignatureHeaderName, "value2");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Dropbox-Signature' header field in the WebHook request but found 2. Please ensure that the request contains exactly one 'X-Dropbox-Signature' header field.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeaderEncoding()
        {
            // Arrange
            _postRequest.Headers.Add(DropboxWebHookReceiver.SignatureHeaderName, "你好世界");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'X-Dropbox-Signature' header value is invalid. It must be a valid hex-encoded string.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnError_IfPostHasInvalidSignature()
        {
            // Arrange
            string invalid = EncodingUtilities.ToHex(Encoding.UTF8.GetBytes("你好世界"));
            _postRequest.Headers.Add(DropboxWebHookReceiver.SignatureHeaderName, invalid);

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'X-Dropbox-Signature' header field does not match the value expected by the 'DropboxWebHookReceiverProxy' receiver. WebHook request is invalid.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            _postRequest.Headers.Add(DropboxWebHookReceiver.SignatureHeaderName, _testSignature);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            List<string> actions = new List<string> { "change" };
            _postRequest.Headers.Add(DropboxWebHookReceiver.SignatureHeaderName, _testSignature);
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
        [InlineData("")]
        [InlineData("challenge=")]
        [InlineData("invalid")]
        public async Task ReceiveAsync_ReturnsError_IfInvalidGetRequest(string query)
        {
            // Arrange
            _getRequest.RequestUri = new Uri("http://localhost?" + query);

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _getRequest);

            // Assert
            Assert.False(actual.IsSuccessStatusCode);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidGetRequest()
        {
            // Arrange
            _getRequest.RequestUri = new Uri("http://localhost?challenge=1234567890");

            // Act
            await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _getRequest);

            // Assert
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
