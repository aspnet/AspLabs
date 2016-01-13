// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public class GitHubWebHookReceiverTests : WebHookReceiverTestsBase<GitHubWebHookReceiver>
    {
        private const string TestContent = "{ \"key\": \"value\" }";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";

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
                    "sha1",
                    "sha1=",
                    "k1=v1;k2=v2",
                };
            }
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed", Justification = "Required by GitHib")]
        public static TheoryData<string, string> ValidPostRequest
        {
            get
            {
                string testSignature;
                byte[] secret = Encoding.UTF8.GetBytes(TestSecret);
                using (var hasher = new HMACSHA1(secret))
                {
                    byte[] data = Encoding.UTF8.GetBytes(TestContent);
                    byte[] testHash = hasher.ComputeHash(data);
                    testSignature = EncodingUtilities.ToHex(testHash);
                }

                return new TheoryData<string, string>
                {
                    { string.Empty, "sha1=" + testSignature },
                    { "id", " sha1=" + testSignature },
                    { "你好", "sha1 =" + testSignature },
                    { "1", "sha1= " + testSignature },
                    { "1234567890", " sha1 = " + testSignature },
                };
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new GitHubWebHookReceiver();
            string expected = "github";

            // Act
            string actual1 = rec.Name;
            string actual2 = GitHubWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
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
            Assert.Equal("Expecting exactly one 'X-Hub-Signature' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'X-Hub-Signature' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, "value2");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Hub-Signature' header field in the WebHook request but found 2. Please ensure that the request contains exactly one 'X-Hub-Signature' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("InvalidPostHeaders")]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeader(string header)
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, header);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Invalid 'X-Hub-Signature' header value. Expecting a value of 'sha1=<value>'.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeaderEncoding()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, "sha1=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'X-Hub-Signature' header value is invalid. It must be a valid hex-encoded string.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignature()
        {
            // Arrange
            Initialize(TestSecret);
            string invalid = EncodingUtilities.ToHex(Encoding.UTF8.GetBytes("invalid"));
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, "sha1=" + invalid);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'X-Hub-Signature' header field does not match the value expected by the 'GitHubWebHookReceiverProxy' receiver. WebHook request is invalid.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("ValidPostRequest")]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson(string id, string header)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, header);
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
        [MemberData("ValidPostRequest")]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoEventHeader(string id, string header)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, header);

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain a 'X-Github-Event' HTTP header indicating the type of event.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), id, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("ValidPostRequest")]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string id, string header)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            List<string> actions = new List<string> { "action1" };
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, header);
            _postRequest.Headers.Add(GitHubWebHookReceiver.EventHeaderName, "action1");
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
        [MemberData("ValidPostRequest")]
        public async Task ReceiveAsync_Succeeds_IfPostPing(string id, string header)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            _postRequest.Headers.Add(GitHubWebHookReceiver.SignatureHeaderName, header);
            _postRequest.Headers.Add(GitHubWebHookReceiver.EventHeaderName, GitHubWebHookReceiver.PingEvent);

            // Act
            HttpResponseMessage response = await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), id, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
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

            _postRequest = new HttpRequestMessage() { Method = HttpMethod.Post };
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }
    }
}
