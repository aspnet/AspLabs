// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SlackWebHookReceiverTests
    {
        private const string TestReceiver = "Test";
        private const string TestSecret = "12345678901234567890123456789012";

        private HttpConfiguration _config;
        private SettingsDictionary _settings;
        private HttpRequestContext _context;
        private Mock<SlackWebHookReceiver> _receiverMock;
        private HttpRequestMessage _postRequest;

        public SlackWebHookReceiverTests()
        {
            _settings = new SettingsDictionary();
            _settings["MS_WebHookReceiverSecret_Slack"] = TestSecret;

            _config = HttpConfigurationMock.Create(new Dictionary<Type, object> { { typeof(SettingsDictionary), _settings } });
            _context = new HttpRequestContext { Configuration = _config };

            _receiverMock = new Mock<SlackWebHookReceiver> { CallBase = true };

            _postRequest = new HttpRequestMessage(HttpMethod.Post, "https://some.ssl.host");
            _postRequest.SetRequestContext(_context);
        }

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

        [Theory]
        [MemberData("Texts")]
        public void GetSubtext_GetsCorrectText(string trigger, string text, string expected)
        {
            // Act
            string actual = SlackWebHookReceiver.GetSubtext(trigger, text);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotUsingHttps()
        {
            // Arrange
            _postRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'SlackWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotFormData()
        {
            // Arrange
            _postRequest.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as HTML Form Data.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasInvalidToken()
        {
            // Arrange
            _postRequest.Content = new StringContent("token=invalid", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'token' parameter provided in the HTTP request did not match the expected value.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoAction()
        {
            // Arrange
            _postRequest.Content = new StringContent("token=" + TestSecret, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'trigger_word' property.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            WebHooksConfig.Initialize(_config);
            List<string> actions = new List<string> { "trigger: hello!" };
            string content = "token=" + TestSecret + "&trigger_word=trigger:+hello!";
            _postRequest.Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");
            _receiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", TestReceiver, _context, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            _receiverMock.Verify();
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
            HttpRequestMessage req = new HttpRequestMessage { Method = new HttpMethod(method) };
            req.SetRequestContext(_context);

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, req);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, req, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }
    }
}
