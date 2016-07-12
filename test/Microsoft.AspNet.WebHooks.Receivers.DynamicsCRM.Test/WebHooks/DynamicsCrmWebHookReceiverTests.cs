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
    public class DynamicsCrmWebHookReceiverTests : WebHookReceiverTestsBase<DynamicsCrmWebHookReceiver>
    {
        private const string TestId = "";
        private const string TestSecret = "0123456789abcdef0123456789abcdef";
        private const string TestHost = "https://ssl.example.invalid/";
        private const string TestEndpoint = TestHost + "?code=" + TestSecret;
        private const string TestContentType = "application/json";
        private const string TestContent = "{\"MessageName\":\"Create\",\"PrimaryEntityName\":\"account\",\"PrimaryEntityId\":\"fedcba98-7654-3210-fedc-ba9876543210\"}";

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
                    "k1=v1;k2=v2"
                };
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new DynamicsCrmWebHookReceiver();
            string expected = "dynamicscrm";

            // Act
            string actual1 = rec.Name;
            string actual2 = DynamicsCrmWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotUsingHttps()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.RequestUri = new Uri("http://nonssl.example.invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'DynamicsCrmWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        [InlineData("OPTIONS")]
        public async Task ReceiveAsync_Throws_IfInvalidHttpMethod(string method)
        {
            // Arrange
            Initialize(TestSecret);
            HttpRequestMessage request = new HttpRequestMessage { Method = new HttpMethod(method) };
            request.SetRequestContext(RequestContext);

            // Act
            HttpResponseMessage response = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, request, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(InvalidCodeQueries))]
        public async Task ReceiveAsync_Throws_IfPostHasNoCodeParameter(string query)
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.RequestUri = new Uri(TestHost + "?" + query);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

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
            _postRequest.RequestUri = new Uri(TestHost + "?code=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'code' query parameter provided in the HTTP request did not match the expected value.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("k=v", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(
                () => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            Initialize(TestSecret);
            List<string> actions = new List<string> { "Create" };
            ReceiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", TestId, RequestContext, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            ReceiverMock.Verify();
        }

        public override void Initialize(string config)
        {
            base.Initialize(config);

            _postRequest = new HttpRequestMessage(HttpMethod.Post, TestEndpoint);
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, TestContentType);
        }
    }
}
