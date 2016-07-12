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
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramWebHookReceiverTests : WebHookReceiverTestsBase<InstagramWebHookReceiver>
    {
        private const string TestContent = "[ { \"object\": \"not1\" }, { \"object\": \"not2\" } ]";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestAddress = "https://localhost/";
        private const string TestChallenge = "1234567890";

        private HttpRequestMessage _getRequest;
        private HttpRequestMessage _postRequest;
        private string _signature;

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed", Justification = "This is how Instagram supports WebHooks.")]
        public InstagramWebHookReceiverTests()
        {
            byte[] secret = Encoding.UTF8.GetBytes(TestSecret);
            using (var hasher = new HMACSHA1(secret))
            {
                byte[] data = Encoding.UTF8.GetBytes(TestContent);
                byte[] testHash = hasher.ComputeHash(data);
                _signature = EncodingUtilities.ToHex(testHash);
            }
        }
        public static TheoryData<string, string[]> InstagramData
        {
            get
            {
                return new TheoryData<string, string[]>
                {
                    { "[ { \"object\": \"test1\" }]", new[] { "test1" } },
                    { "[ { \"object\": \"你好世界\" }]", new[] { "你好世界" } },
                    { "[ { \"object\": \"test1\" }, { \"object\": \"你好世界\" } ]", new[] { "test1", "你好世界" } },
                };
            }
        }

        public static TheoryData<string> InvalidInstagramData
        {
            get
            {
                return new TheoryData<string>
                {
                   "[ { \"object\": { \"你好\": \"世界\" } }]",
                   "[ { \"object\": [ { \"你好\": \"世界\" } ] }]",
                };
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new InstagramWebHookReceiver();
            string expected = "instagram";

            // Act
            string actual1 = rec.Name;
            string actual2 = InstagramWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
        }

        [Theory]
        [MemberData(nameof(InvalidInstagramData))]
        public async Task GetActions_Throws_IfInvalidData(string invalid)
        {
            Initialize(TestSecret);
            InstagramReceiverMock mock = new InstagramReceiverMock();
            JArray data = JArray.Parse(invalid);

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => mock.GetActions(data, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.StartsWith("Could not parse WebHook data: ", error.Message);
        }

        [Theory]
        [MemberData(nameof(InstagramData))]
        public void GetActions_ExtractsActions(string valid, IEnumerable<string> actions)
        {
            // Arrange
            Initialize(TestSecret);
            InstagramReceiverMock mock = new InstagramReceiverMock();
            JArray data = JArray.Parse(valid);

            // Act
            IEnumerable<string> actual = mock.GetActions(data, _postRequest);

            // Assert
            Assert.Equal(actions, actual);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotUsingHttps()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'InstagramWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfGetIsNotUsingHttps()
        {
            // Arrange
            Initialize(TestSecret);
            _getRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _getRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'InstagramWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasNoSignatureHeader()
        {
            // Act
            Initialize(TestSecret);
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'x-hub-signature' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'x-hub-signature' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(InstagramWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(InstagramWebHookReceiver.SignatureHeaderName, "value2");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'x-hub-signature' header field in the WebHook request but found 2. Please ensure that the request contains exactly one 'x-hub-signature' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasInvalidSignatureHeaderEncoding()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(InstagramWebHookReceiver.SignatureHeaderName, "你好世界");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'x-hub-signature' header value is invalid. It must be a valid base64-encoded string.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignature()
        {
            // Arrange
            Initialize(TestSecret);
            string invalid = EncodingUtilities.ToHex(Encoding.UTF8.GetBytes("你好世界"));
            _postRequest.Headers.Add(InstagramWebHookReceiver.SignatureHeaderName, invalid);

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'x-hub-signature' header field does not match the value expected by the 'InstagramWebHookReceiverProxy' receiver. WebHook request is invalid.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(InstagramWebHookReceiver.SignatureHeaderName, _signature);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
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
            List<string> actions = new List<string> { "not1", "not2" };
            _postRequest.Headers.Add(InstagramWebHookReceiver.SignatureHeaderName, _signature);
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
        [InlineData("")]
        [InlineData("hub.challenge=")]
        [InlineData("invalid")]
        public async Task ReceiveAsync_ReturnsError_IfInvalidGetRequest(string query)
        {
            // Arrange
            Initialize(TestSecret);
            _getRequest.RequestUri = new Uri("https://localhost?" + query);

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _getRequest);

            // Assert
            Assert.False(actual.IsSuccessStatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidGetRequest(string id)
        {
            // Act
            Initialize(GetConfigValue(id, TestSecret));
            await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _getRequest);

            // Assert
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), id, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        [InlineData("OPTIONS")]
        public async Task ReceiveAsync_ReturnsError_IfInvalidMethod(string method)
        {
            // Arrange
            Initialize(TestSecret);
            HttpRequestMessage req = new HttpRequestMessage { Method = new HttpMethod(method) };
            req.SetRequestContext(RequestContext);

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, req);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, req, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        public override void Initialize(string config)
        {
            base.Initialize(config);

            _getRequest = new HttpRequestMessage(HttpMethod.Get, TestAddress + "?hub.challenge=" + TestChallenge);
            _getRequest.SetRequestContext(RequestContext);

            _postRequest = new HttpRequestMessage(HttpMethod.Post, TestAddress);
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }

        private class InstagramReceiverMock : InstagramWebHookReceiver
        {
            public new IEnumerable<string> GetActions(JArray data, HttpRequestMessage request)
            {
                return base.GetActions(data, request);
            }
        }
    }
}
