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
    public class SlackWebHookReceiverTests : WebHookReceiverTestsBase<SlackWebHookReceiver>
    {
        private const string TestTriggerContent = "token=" + TestSecret + "&trigger_word=trigger:+hello!";
        private const string TestSlashContent = "token=" + TestSecret + "&command=hello!";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";

        private HttpRequestMessage _postRequest;

        public static TheoryData<string, string, string> Texts
        {
            get
            {
                return new TheoryData<string, string, string>
                {
                    { string.Empty, null, string.Empty },
                    { string.Empty, string.Empty, string.Empty },
                    { "你好", "你好世界something", "世界something" },
                    { "你好", "你好something", "something" },
                    { "你好", "你好\r\t\nsomething   \r\t\n", "something" },
                    { "trigger", null, string.Empty },
                    { "trigger", string.Empty, string.Empty },
                    { "trigger", "text", "text" },
                    { "trigger", "TriggerText", "Text" },
                    { "TRIGGER", "triggerText", "Text" },
                    { "TRIGGER", "TRIGGER   Text   ", "Text" },
                };
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new SlackWebHookReceiver();
            var expected = "slack";

            // Act
            var actual1 = rec.Name;
            var actual2 = SlackWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
        }

        [Theory]
        [MemberData(nameof(Texts))]
        public void GetSubtext_GetsCorrectText(string trigger, string text, string expected)
        {
            // Act
            var actual = SlackWebHookReceiver.GetSubtext(trigger, text);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotUsingHttps()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'SlackWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
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
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as HTML form URL-encoded data.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasInvalidToken()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("token=invalid", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            var error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'token' parameter provided in the HTTP request did not match the expected value.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoAction()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("token=" + TestSecret, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            var error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'command' property indicating a slash command or contained an empty 'trigger_word' parameter indicating an outgoing WebHook.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidTriggerPostRequest(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            var actions = new List<string> { "trigger: hello!" };
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
        public async Task ReceiveAsync_Succeeds_IfValidSlashPostRequest(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            _postRequest.Content = new StringContent(TestSlashContent, Encoding.UTF8, "application/x-www-form-urlencoded");

            var actions = new List<string> { "hello!" };
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

        public override void Initialize(string config)
        {
            base.Initialize(config);

            _postRequest = new HttpRequestMessage(HttpMethod.Post, "https://some.ssl.host");
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestTriggerContent, Encoding.UTF8, "application/x-www-form-urlencoded");
        }
    }
}
