// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class CustomWebHookReceiverTests
    {
        private const string TestContent = "{\r\n  \"Id\": \"1234567890\",\r\n  \"Attempt\": 1,\r\n  \"Properties\": {\r\n    \"p1\": \"pv1\"\r\n  },\r\n  \"Notifications\": [\r\n    {\r\n      \"Action\": \"a1\",\r\n      \"d1\": \"dv1\"\r\n    },\r\n    {\r\n      \"Action\": \"a1\",\r\n      \"d2\": \"http://localhost/\"\r\n    }\r\n  ]\r\n}";
        private const string TestReceiver = "Test";
        private const string TestSecret = "12345678901234567890123456789012";

        private HttpConfiguration _config;
        private SettingsDictionary _settings;
        private HttpRequestContext _context;
        private Mock<CustomWebHookReceiver> _receiverMock;
        private HttpRequestMessage _getRequest;
        private HttpRequestMessage _postRequest;

        public CustomWebHookReceiverTests()
        {
            _settings = new SettingsDictionary();
            _settings["MS_WebHookReceiverSecret_Custom_Test"] = TestSecret;

            _config = HttpConfigurationMock.Create(new Dictionary<Type, object> { { typeof(SettingsDictionary), _settings } });
            _context = new HttpRequestContext { Configuration = _config };

            _receiverMock = new Mock<CustomWebHookReceiver>(_config) { CallBase = true };

            _getRequest = new HttpRequestMessage();
            _getRequest.SetRequestContext(_context);

            _postRequest = new HttpRequestMessage() { Method = HttpMethod.Post };
            _postRequest.SetRequestContext(_context);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }

        public static TheoryData<string[], string[]> ReceiverNames
        {
            get
            {
                return new TheoryData<string[], string[]>
                {
                    { new string[0], new string[0] },
                    { new string[] { string.Empty }, new string[0] },
                    { new string[] { "MS_WebHookReceiverSecret_Custom" }, new string[0] },
                    { new string[] { "MS_WebHookReceiverSecret_Custom_" }, new string[0] },
                    { new string[] { "WebHookReceiverSecret_Custom" }, new string[0] },
                    { new string[] { "Hello" }, new string[0] },
                    { new string[] { "MS_WebHookReceiverSecret_Custom_Net" }, new string[] { "net" } },
                    { new string[] { "MS_WebHookReceiverSecret_Custom_你好" }, new string[] { "你好" } },
                    { new string[] { "MS_WebHookReceiverSecret_Custom_Net", "MS_WebHookReceiverSecret_Custom_你好" }, new string[] { "net", "你好" } },
                    { new string[] { "MS_WebHookReceiverSecret_Custom_Net", "other", "MS_WebHookReceiverSecret_Custom_你好" }, new string[] { "net", "你好" } },
                    { new string[] { "other1", "MS_WebHookReceiverSecret_Custom_你好", "other2" }, new string[] { "你好" } },
                };
            }
        }

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

        public static TheoryData<string> ValidPostRequest
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

                return new TheoryData<string>
                {
                    "sha256=" + testSignature,
                    " sha256=" + testSignature,
                    "sha256 =" + testSignature,
                    "sha256= " + testSignature,
                    " sha256 = " + testSignature,
                };
            }
        }

        [Theory]
        [MemberData("InvalidCustomData")]
        public async Task GetActions_Throws_IfInvalidData(string invalid)
        {
            ReceiverMock mock = new ReceiverMock(_config);
            JObject data = JObject.Parse(invalid);

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => mock.GetActions(_postRequest, data));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.StartsWith("Could not parse WebHook data: ", error.Message);
        }

        [Theory]
        [MemberData("CustomData")]
        public void GetActions_ExtractsActions(string valid, IEnumerable<string> actions)
        {
            // Arrange
            ReceiverMock mock = new ReceiverMock(_config);
            JObject data = JObject.Parse(valid);

            // Act
            IEnumerable<string> actual = mock.GetActions(_postRequest, data);

            // Assert
            Assert.Equal(actions, actual);
        }

        [Fact]
        public void CustomWebHookReceiver_Config_InitializesNames()
        {
            // Arrange
            CustomWebHookReceiver receiver = new CustomWebHookReceiver(_config);

            // Act
            IEnumerable<string> actual = receiver.Names;

            // Assert
            Assert.Equal("test", actual.Single());
        }

        [Theory]
        [MemberData("ReceiverNames")]
        public void GetNames_PicksOutExpectedNames(IEnumerable<string> secrets, IEnumerable<string> expected)
        {
            // Arrange
            _settings.Clear();
            foreach (string secret in secrets)
            {
                _settings.Add(secret, "some secret key");
            }

            // Act
            IEnumerable<string> actual = CustomWebHookReceiver.GetNames(_config);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasNoSignatureHeader()
        {
            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'ms-signature' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'ms-signature' header field.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, "value2");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'ms-signature' header field in the WebHook request but found 2. Please ensure that the request contains exactly one 'ms-signature' header field.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("InvalidPostHeaders")]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeader(string header)
        {
            // Arrange
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, header);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Invalid 'ms-signature' header value. Expecting a value of 'sha256=<value>'.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeaderEncoding()
        {
            // Arrange
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, "sha256=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'ms-signature' header value is invalid. It must be a valid hex-encoded string.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignature()
        {
            // Arrange
            string invalid = EncodingUtilities.ToHex(Encoding.UTF8.GetBytes("invalid"));
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, "sha256=" + invalid);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'ms-signature' header field does not match the value expected by the 'CustomWebHookReceiverProxy' receiver. WebHook request is invalid.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("ValidPostRequest")]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson(string header)
        {
            // Arrange
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, header);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData("ValidPostRequest")]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string header)
        {
            // Arrange
            WebHooksConfig.Initialize(_config);
            List<string> actions = new List<string> { "a1", "a1" };
            _postRequest.Headers.Add(CustomWebHookReceiver.SignatureHeaderName, header);
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
        [InlineData("")]
        [InlineData("echo=")]
        [InlineData("invalid")]
        public async Task ReceiveAsync_ReturnsError_IfInvalidGetRequest(string query)
        {
            // Arrange
            _getRequest.RequestUri = new Uri("http://localhost?" + query);

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _getRequest);

            // Assert
            Assert.False(actual.IsSuccessStatusCode);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidGetRequest()
        {
            // Arrange
            _getRequest.RequestUri = new Uri("http://localhost?echo=1234567890");

            // Act
            await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _getRequest);

            // Assert
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _getRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
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

        private class ReceiverMock : CustomWebHookReceiver
        {
            public ReceiverMock(HttpConfiguration config)
                : base(config)
            {
            }

            public new IEnumerable<string> GetActions(HttpRequestMessage request, JObject data)
            {
                return base.GetActions(request, data);
            }
        }
    }
}
