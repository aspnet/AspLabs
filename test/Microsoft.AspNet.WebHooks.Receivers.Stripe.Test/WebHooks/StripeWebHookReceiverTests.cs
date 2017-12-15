// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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
    public class StripeWebHookReceiverTests : WebHookReceiverTestsBase<StripeWebHookReceiver>
    {
        private const string ExtraHeaderContent =
            "  v1=5257a869e7ecebeda32affa62cdca3fa51cad7e77a0e56ff536d0ce8e108d8bd, " +
            "v0=6ffbb59b2300aae63f272406069a9788598b792a944a07aba816edb039989a39  ";
        private const string TestContent = "{ \"type\": \"action\", \"id\": \"" + TestStripeId + "\" }";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestStripeId = "12345";
        private const string TestTimestamp = "1492774577";

        private static readonly string TestSignatureHeader = GetSignatureHeader(TestContent);
        private HttpRequestMessage _postRequest;

        public static TheoryData<string> HeadersWithMissingValues
        {
            get
            {
                return new TheoryData<string>
                {
                    string.Empty,
                    $"{StripeWebHookReceiver.TimestampKey}={TestTimestamp} ",
                    ExtraHeaderContent,
                    "a=b, b=c, c=d, e=f",
                };
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new StripeWebHookReceiver();
            var expected = "stripe";

            // Act
            var actual1 = rec.Name;
            var actual2 = StripeWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasNoSignatureHeader()
        {
            // Arrange
            Initialize(TestSecret);
            var expectedMessage = "Expecting exactly one 'Stripe-Signature' header field in the WebHook request " +
                "but found 0. Please ensure that the request contains exactly one 'Stripe-Signature' header field.";

            // Act
            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await exception.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expectedMessage, error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, "value2");
            var expectedMessage = "Expecting exactly one 'Stripe-Signature' header field in the WebHook request " +
                "but found 2. Please ensure that the request contains exactly one 'Stripe-Signature' header field.";

            // Act
            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await exception.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expectedMessage, error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [InlineData("你好世界")]
        [InlineData("invalid")]
        [InlineData("==")]
        [InlineData(",=,=,")]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeader(string header)
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, header);
            var expectedMessage = "The 'Stripe-Signature' header value is invalid. It must be formatted as " +
                "key=value pairs separated by commas.";

            // Act
            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await exception.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expectedMessage, error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(HeadersWithMissingValues))]
        public async Task ReceiveAsync_Throws_IfPostHasSignatureHeaderWithMissingValues(string header)
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, header);
            var expectedMessage = $"The '{StripeWebHookReceiver.SignatureHeaderName}' header value is invalid. It " +
                $"must contain timestamp ('{StripeWebHookReceiver.TimestampKey}') and signature " +
                $"('{StripeWebHookReceiver.SignatureKey}') values.";

            // Act
            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await exception.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expectedMessage, error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureEncoding()
        {
            // Arrange
            Initialize(TestSecret);
            var header = $"{StripeWebHookReceiver.TimestampKey}={TestTimestamp}, " +
                $"{StripeWebHookReceiver.SignatureKey}=invalid";
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, header);
            var expectedMessage = $"The '{StripeWebHookReceiver.SignatureHeaderName}' header value is invalid. It " +
                $"must contain a valid hex-encoded signature ('{StripeWebHookReceiver.SignatureKey}') value.";

            // Act
            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await exception.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expectedMessage, error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnError_IfPostHasIncorrectSignature()
        {
            // Arrange
            Initialize(TestSecret);
            var header = $"{StripeWebHookReceiver.TimestampKey}={TestTimestamp}, {ExtraHeaderContent}";
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, header);
            var expectedMessage = "The WebHook signature provided by the 'Stripe-Signature' header field does not " +
                "match the value expected by the 'StripeWebHookReceiverProxy' receiver. WebHook request is invalid.";

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            var error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expectedMessage, error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, TestSignatureHeader);

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoId()
        {
            // Arrange
            var content = "{ \"type\": \"action\" }";
            Initialize(TestSecret);
            _postRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, GetSignatureHeader(content));

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            var error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'id' property.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_WithoutCallingHandler_IfTestId()
        {
            // Arrange
            var content = "{ \"type\": \"action\", \"id\": \"" + StripeWebHookReceiver.TestId + "\" }";
            Initialize(TestSecret);
            _postRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, GetSignatureHeader(content));

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_CallingHandler_IfTestIdAndTestMode()
        {
            // Arrange
            var content = "{ \"type\": \"action\", \"id\": \"" + StripeWebHookReceiver.TestId + "\" }";
            Initialize(TestSecret, inTestMode: true);
            _postRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, GetSignatureHeader(content));

            var actions = new List<string> { "action" };
            ReceiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", string.Empty, RequestContext, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            ReceiverMock.Verify();
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, TestSignatureHeader);

            var actions = new List<string> { "action" };
            ReceiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", id, RequestContext, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _postRequest);

            // Assert
            ReceiverMock.Verify();
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest_WithExtraSignatureHeaderContent()
        {
            // Arrange
            Initialize(TestSecret);
            var header = $" {TestSignatureHeader} , {ExtraHeaderContent} ";
            _postRequest.Headers.Add(StripeWebHookReceiver.SignatureHeaderName, header);

            var actions = new List<string> { "action" };
            ReceiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", TestId, RequestContext, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            ReceiverMock.Verify();
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
            var req = new HttpRequestMessage { Method = new HttpMethod(method) };
            req.SetRequestContext(RequestContext);

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, req);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, req, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        public void Initialize(string config, bool inTestMode)
        {
            Initialize(config);
            Settings.Add(StripeWebHookReceiver.PassThroughTestEvents, inTestMode.ToString());
        }

        public override void Initialize(string config)
        {
            base.Initialize(config);

            _postRequest = new HttpRequestMessage
            {
                Content = new StringContent(TestContent, Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post,
            };

            _postRequest.SetRequestContext(RequestContext);
        }

        private static string GetSignatureHeader(string content)
        {
            var secret = Encoding.UTF8.GetBytes(TestSecret);
            using (var hasher = new HMACSHA256(secret))
            {
                var fullContent = $"{TestTimestamp}.{content}";
                var data = Encoding.UTF8.GetBytes(fullContent);
                var testHash = hasher.ComputeHash(data);
                var signature = EncodingUtilities.ToHex(testHash);

                return $"  {StripeWebHookReceiver.TimestampKey}={TestTimestamp},  " +
                    $"{StripeWebHookReceiver.SignatureKey}={signature}  ";
            }
        }
    }
}
