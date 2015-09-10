// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class StripeWebHookReceiverTests : WebHookReceiverTestsBase<StripeWebHookReceiver>
    {
        private const string TestContent = "{ \"type\": \"action\", \"id\": \"" + TestStripeId + "\" }";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestStripeId = "12345";

        private HttpClient _httpClient;
        private HttpMessageHandlerMock _handlerMock;

        private HttpRequestMessage _postRequest;
        private HttpResponseMessage _stripeResponse;

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("k=v", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            Assert.Equal(0, _handlerMock.Counter);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoId()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("{ \"type\": \"action\" }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'id' property.", error.Message);
            Assert.Equal(0, _handlerMock.Counter);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfTestId()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("{ \"type\": \"action\", \"id\": \"" + StripeWebHookReceiver.TestId + "\" }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Equal(0, _handlerMock.Counter);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasUnknownId()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("{ \"type\": \"action\", \"id\": \"" + TestStripeId + "\" }", Encoding.UTF8, "application/json");
            _handlerMock.Handler = (req, counter) =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            };

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The ID '12345' could not be resolved for an actual event.", error.Message);
            Assert.Equal(1, _handlerMock.Counter);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("ValidIdData")]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            List<string> actions = new List<string> { "action" };
            ReceiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", id, RequestContext, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _postRequest);

            // Assert
            Assert.Equal(1, _handlerMock.Counter);
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
            HttpRequestMessage req = new HttpRequestMessage { Method = new HttpMethod(method) };
            req.SetRequestContext(RequestContext);

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, req);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, req, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public void Dispose_Succeeds()
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            Receiver.Dispose();
            Receiver.Dispose();
        }

        public override void Initialize(string config)
        {
            _handlerMock = new HttpMessageHandlerMock();
            _handlerMock.Handler = (req, counter) =>
            {
                string expected = string.Format(CultureInfo.InvariantCulture, StripeWebHookReceiver.EventUriTemplate, TestStripeId);
                Assert.Equal(req.RequestUri.AbsoluteUri, expected);
                return Task.FromResult(_stripeResponse);
            };

            _httpClient = new HttpClient(_handlerMock);
            ReceiverMock = new Mock<StripeWebHookReceiver>(_httpClient) { CallBase = true };

            base.Initialize(config);

            _stripeResponse = new HttpResponseMessage();
            _stripeResponse.Content = new StringContent("{ \"type\": \"action\" }", Encoding.UTF8, "application/json");

            _postRequest = new HttpRequestMessage { Method = HttpMethod.Post };
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }
    }
}
