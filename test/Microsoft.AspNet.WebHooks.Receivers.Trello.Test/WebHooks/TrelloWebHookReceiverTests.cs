// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class TrelloWebHookReceiverTests : WebHookReceiverTestsBase<TrelloWebHookReceiver>
    {
        private const string TestContent = "{ \"key\": \"value\" }";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestAddress = "http://localhost/";

        private HttpRequestMessage _headRequest;
        private HttpRequestMessage _postRequest;
        private string _signature;

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed", Justification = "This is how Trello supports WebHooks.")]
        public TrelloWebHookReceiverTests()
        {
            var secret = Encoding.UTF8.GetBytes(TestSecret);
            using (var hasher = new HMACSHA1(secret))
            {
                var data = Encoding.UTF8.GetBytes(TestContent);
                var requestUri = Encoding.UTF8.GetBytes(TestAddress);
                var combinedBytes = new byte[data.Length + requestUri.Length];
                Buffer.BlockCopy(data, 0, combinedBytes, 0, data.Length);
                Buffer.BlockCopy(requestUri, 0, combinedBytes, data.Length, requestUri.Length);
                var testHash = hasher.ComputeHash(combinedBytes);
                _signature = EncodingUtilities.ToBase64(testHash, uriSafe: false);
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new TrelloWebHookReceiver();
            var expected = "trello";

            // Act
            var actual1 = rec.Name;
            var actual2 = TrelloWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasNoSignatureHeader()
        {
            // Act
            Initialize(TestSecret);
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'x-trello-webhook' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'x-trello-webhook' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(TrelloWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(TrelloWebHookReceiver.SignatureHeaderName, "value2");

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'x-trello-webhook' header field in the WebHook request but found 2. Please ensure that the request contains exactly one 'x-trello-webhook' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasInvalidSignatureHeaderEncoding()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(TrelloWebHookReceiver.SignatureHeaderName, "你好世界");

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'x-trello-webhook' header value is invalid. It must be a valid base64-encoded string.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignature()
        {
            // Arrange
            Initialize(TestSecret);
            var invalid = EncodingUtilities.ToBase64(Encoding.UTF8.GetBytes("你好世界"), uriSafe: false);
            _postRequest.Headers.Add(TrelloWebHookReceiver.SignatureHeaderName, invalid);

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            var error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'x-trello-webhook' header field does not match the value expected by the 'TrelloWebHookReceiverProxy' receiver. WebHook request is invalid.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(TrelloWebHookReceiver.SignatureHeaderName, _signature);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            var actions = new List<string> { "change" };
            _postRequest.Headers.Add(TrelloWebHookReceiver.SignatureHeaderName, _signature);
            ReceiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", id, RequestContext, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _postRequest);

            // Assert
            ReceiverMock.Verify();
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidHeadRequest(string id)
        {
            // Act
            Initialize(GetConfigValue(id, TestSecret));
            await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _headRequest);

            // Assert
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), id, RequestContext, _headRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        [InlineData("OPTIONS")]
        public async Task ReceiveAsync_ReturnsError_IfInvalidMethod(string method)
        {
            // Arrange
            Initialize(TestSecret);
            var req = new HttpRequestMessage { Method = new HttpMethod(method) };
            req.SetRequestContext(RequestContext);

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, req);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, req, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        public override void Initialize(string config)
        {
            base.Initialize(config);

            _headRequest = new HttpRequestMessage() { Method = HttpMethod.Head };
            _headRequest.SetRequestContext(RequestContext);

            _postRequest = new HttpRequestMessage(HttpMethod.Post, TestAddress);
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }
    }
}
