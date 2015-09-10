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
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class PaypalWebHookReceiverTests : WebHookReceiverTestsBase<PaypalWebHookReceiver>
    {
        private const string TestContent = "{ \"key\": \"value\", \"event_type\": \"action1\" }";
        private const string TestId = "";
        private const string TestSecret = "NotUsed";

        private HttpRequestMessage _postRequest;

        [Fact]
        public void Constructor_Throws_IfNoConfig()
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => new PaypalWebHookReceiver());

            // Assert
            Assert.StartsWith("Initialization of the Paypal SDK failed:", ex.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfNoRequestBody()
        {
            // Arrange
            Initialize(TestSecret);
            ReceiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", RequestContext, ItExpr.IsAny<NameValueCollection>(), string.Empty)
                .Returns(true)
                .Verifiable();
            _postRequest.Content = null;

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(string.Empty, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request entity body cannot be empty.", error.Message);
            ReceiverMock.Verify();
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            Initialize(TestSecret);
            ReceiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", RequestContext, ItExpr.IsAny<NameValueCollection>(), TestContent)
                .Returns(true)
                .Verifiable();
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            ReceiverMock.Verify();
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoEventParameter()
        {
            // Arrange
            Initialize(TestSecret);
            ReceiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", RequestContext, ItExpr.IsAny<NameValueCollection>(), "{ }")
                .Returns(true)
                .Verifiable();
            _postRequest.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'event_type' property.", error.Message);
            ReceiverMock.Verify();
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfValidationFails()
        {
            // Arrange
            Initialize(TestSecret);
            ReceiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", RequestContext, ItExpr.IsAny<NameValueCollection>(), TestContent)
                .Returns(false)
                .Verifiable();

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The received WebHook is not valid.", error.Message);
            ReceiverMock.Verify();
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            Initialize(TestSecret);
            List<string> actions = new List<string> { "action1" };
            ReceiverMock.Protected()
                .Setup<bool>("ValidateReceivedEvent", RequestContext, ItExpr.IsAny<NameValueCollection>(), TestContent)
                .Returns(true)
                .Verifiable();
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
            ReceiverMock = new Mock<PaypalWebHookReceiver>(false) { CallBase = true };
            base.Initialize(config);

            _postRequest = new HttpRequestMessage() { Method = HttpMethod.Post };
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }
    }
}
