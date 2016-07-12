// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public class MailChimpWebHookReceiverTests : WebHookReceiverTestsBase<MailChimpWebHookReceiver>
    {
        private const string TestContent = "type=hello";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestAddress = "https://some.ssl.host?code=" + TestSecret;

        private HttpRequestMessage _getRequest;
        private HttpRequestMessage _postRequest;

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
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new MailChimpWebHookReceiver();
            string expected = "mailchimp";

            // Act
            string actual1 = rec.Name;
            string actual2 = MailChimpWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
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
            Assert.Equal("The WebHook receiver 'MailChimpWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(InvalidCodeQueries))]
        public async Task ReceiveAsync_Throws_IfPostHasNoCodeParameter(string query)
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.RequestUri = new Uri("https://some.no.ssl.host?" + query);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook verification request must contain a 'code' query parameter.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasWrongCodeParameter()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.RequestUri = new Uri("https://some.no.ssl.host?code=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'code' query parameter provided in the HTTP request did not match the expected value.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotFormData()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as HTML Form Data.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoAction()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("k=v", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'type' property.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            List<string> actions = new List<string> { "hello" };
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
        public async Task ReceiveAsync_Throws_IfGetIsNotUsingHttps()
        {
            // Arrange
            Initialize(TestSecret);
            _getRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _getRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'MailChimpWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(InvalidCodeQueries))]
        public async Task ReceiveAsync_Throws_IfGetHasNoCodeParameter(string query)
        {
            // Arrange
            Initialize(TestSecret);
            _getRequest.RequestUri = new Uri("https://some.no.ssl.host?" + query);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _getRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook verification request must contain a 'code' query parameter.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfGetHasWrongCodeParameter()
        {
            // Arrange
            Initialize(TestSecret);
            _getRequest.RequestUri = new Uri("https://some.no.ssl.host?code=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _getRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'code' query parameter provided in the HTTP request did not match the expected value.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidGet(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _getRequest);

            // Assert
            Assert.Equal(actual.StatusCode, HttpStatusCode.OK);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), id, RequestContext, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
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

            _getRequest = new HttpRequestMessage(HttpMethod.Get, TestAddress);
            _getRequest.SetRequestContext(RequestContext);

            _postRequest = new HttpRequestMessage(HttpMethod.Post, TestAddress);
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
        }
    }
}
