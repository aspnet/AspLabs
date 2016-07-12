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
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class CustomWebHookReceiverTests : WebHookReceiverTestsBase<CustomWebHookReceiver>
    {
        private const string TestContent = "{\r\n  \"Id\": \"1234567890\",\r\n  \"Attempt\": 1,\r\n  \"Properties\": {\r\n    \"p1\": \"pv1\"\r\n  },\r\n  \"Notifications\": [\r\n    {\r\n      \"Action\": \"a1\",\r\n      \"d1\": \"dv1\"\r\n    },\r\n    {\r\n      \"Action\": \"a1\",\r\n      \"d2\": \"http://localhost/\"\r\n    }\r\n  ]\r\n}";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";

        private HttpRequestMessage _getRequest;
        private HttpRequestMessage _postRequest;

        public static TheoryData<string> InvalidPostHeaders
        {
            get
            {
                return new TheoryData<string>
                {
                    string.Empty,
                    "=",
                    "==",
                    "invalid",
                    "sha256",
                    "sha256=",
                    "k1=v1;k2=v2",
                };
            }
        }

        public static TheoryData<string, string[]> CustomData
        {
            get
            {
                return new TheoryData<string, string[]>
                {
                    { "{\"Attempt\":1,\"Notifications\":[{\"Action\":\"a1\"}]}", new[] { "a1" } },
                    { "{\"Attempt\":1,\"Notifications\":[{\"Action\":\"a1\"},{\"Action\":\"你好世界\"},]}", new[] { "a1", "你好世界" } },
                    { "{\"Attempt\":1,\"Notifications\":[{\"Action\":\"a1\"},{}]}", new[] { "a1" } },
                    { "{\"Attempt\":1,\"Notifications\":[{},{\"Action\":\"a1\"}]}", new[] { "a1" } },
                };
            }
        }

        public static TheoryData<string> InvalidCustomData
        {
            get
            {
                return new TheoryData<string>
                {
                   "{ \"Attempt\": 1, \"Notifications\": \"i n v a l i d\" }",
                   "{ \"Attempt\": 1, \"Notifications\": [ { \"Action\": { } } ] }",
                };
            }
        }

        public static TheoryData<string, string> ValidPostRequest
        {
            get
            {
                string testSignature;
                byte[] secret = Encoding.UTF8.GetBytes(TestSecret);
                using (var hasher = new HMACSHA256(secret))
                {
                    byte[] data = Encoding.UTF8.GetBytes(TestContent);
                    byte[] testHash = hasher.ComputeHash(data);
                    testSignature = EncodingUtilities.ToHex(testHash);
                }

                return new TheoryData<string, string>
                {
                    { string.Empty, "sha256=" + testSignature },
                    { "id", " sha256=" + testSignature },
                    { "你好", "sha256 =" + testSignature },
                    { "1", "sha256= " + testSignature },
                    { "1234567890", " sha256 = " + testSignature },
                };
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new CustomWebHookReceiver();
            string expected = "custom";

            // Act
            string actual1 = rec.Name;
            string actual2 = CustomWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
        }

        [Theory]
        [MemberData(nameof(InvalidCustomData))]
        public async Task GetActions_Throws_IfInvalidData(string invalid)
        {
            Initialize(TestSecret);
            CustomReceiverMock mock = new CustomReceiverMock();
            JObject data = JObject.Parse(invalid);

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => mock.GetActions(data, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.StartsWith("Could not parse WebHook data: ", error.Message);
        }

        [Theory]
        [MemberData(nameof(CustomData))]
        public void GetActions_ExtractsActions(string valid, IEnumerable<string> actions)
        {
            // Arrange
            Initialize(TestSecret);
            CustomReceiverMock mock = new CustomReceiverMock();
            JObject data = JObject.Parse(valid);

            // Act
            IEnumerable<string> actual = mock.GetActions(data, _postRequest);

            // Assert
            Assert.Equal(actions, actual);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasNoSignatureHeader()
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'ms-signature' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'ms-signature' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, "value2");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'ms-signature' header field in the WebHook request but found 2. Please ensure that the request contains exactly one 'ms-signature' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(InvalidPostHeaders))]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeader(string header)
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, header);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Invalid 'ms-signature' header value. Expecting a value of 'sha256=<value>'.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeaderEncoding()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, "sha256=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'ms-signature' header value is invalid. It must be a valid hex-encoded string.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignature()
        {
            // Arrange
            Initialize(TestSecret);
            string invalid = EncodingUtilities.ToHex(Encoding.UTF8.GetBytes("invalid"));
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, "sha256=" + invalid);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'ms-signature' header field does not match the value expected by the 'CustomWebHookReceiverProxy' receiver. WebHook request is invalid.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidPostRequest))]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson(string id, string header)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, header);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(id, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), id, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidPostRequest))]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string id, string header)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            List<string> actions = new List<string> { "a1", "a1" };
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, header);
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
        [InlineData("echo=")]
        [InlineData("invalid")]
        public async Task ReceiveAsync_ReturnsError_IfInvalidGetRequest(string query)
        {
            // Arrange
            Initialize(TestSecret);
            _getRequest.RequestUri = new Uri("http://localhost?" + query);

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _getRequest);

            // Assert
            Assert.False(actual.IsSuccessStatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidGetRequest()
        {
            // Arrange
            Initialize(TestSecret);
            _getRequest.RequestUri = new Uri("http://localhost?echo=1234567890");

            // Act
            await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _getRequest);

            // Assert
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [InlineData("HEAD")]
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

            _getRequest = new HttpRequestMessage();
            _getRequest.SetRequestContext(RequestContext);

            _postRequest = new HttpRequestMessage() { Method = HttpMethod.Post };
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }

        private class CustomReceiverMock : CustomWebHookReceiver
        {
            public new IEnumerable<string> GetActions(JObject data, HttpRequestMessage request)
            {
                return base.GetActions(data, request);
            }
        }
    }
}
