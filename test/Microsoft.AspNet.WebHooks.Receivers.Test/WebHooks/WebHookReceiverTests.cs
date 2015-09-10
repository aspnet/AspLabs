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
using System.Web.Http.Controllers;
using System.Xml.Linq;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Mocks;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookReceiverTests
    {
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string SecretPrefix = "MS_WebHookReceiverSecret_";

        private ILogger _logger;
        private SettingsDictionary _settings;
        private IWebHookReceiverConfig _config;
        private HttpConfiguration _httpConfig;

        private HttpRequestMessage _request;
        private HttpRequestContext _context;
        private WebHookReceiverMock _receiverMock;

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

        public static TheoryData<byte[], byte[], bool> ByteCompareData
        {
            get
            {
                return new TheoryData<byte[], byte[], bool>
                {
                    { null, null, true },
                    { new byte[0], null, false },
                    { null, new byte[0], false },
                    { new byte[0], new byte[0], true },
                    { Encoding.UTF8.GetBytes("123"), Encoding.UTF8.GetBytes("1 2 3"), false },
                    { Encoding.UTF8.GetBytes("你好世界"), Encoding.UTF8.GetBytes("你好世界"), true },
                    { Encoding.UTF8.GetBytes("你好"), Encoding.UTF8.GetBytes("世界"), false },
                    { Encoding.UTF8.GetBytes(new string('a', 8 * 1024)), Encoding.UTF8.GetBytes(new string('a', 8 * 1024)), true },
                };
            }
        }

        public static TheoryData<string, string, bool> StringCompareData
        {
            get
            {
                return new TheoryData<string, string, bool>
                {
                    { null, null, true },
                    { string.Empty, null, false },
                    { null, string.Empty, false },
                    { string.Empty, string.Empty, true },
                    { "123", "1 2 3", false },
                    { "你好世界", "你好世界", true },
                    { "你好", "世界", false },
                    { new string('a', 8 * 1024), new string('a', 8 * 1024), true },
                };
            }
        }

        public static TheoryData<string> ValidIdData
        {
            get
            {
                return new TheoryData<string>
                {
                    { string.Empty },
                    { "id" },
                    { "你好" },
                    { "1" },
                    { "1234567890" },
                };
            }
        }
        public static TheoryData<string, string, string, int, int> ValidConfigData
        {
            get
            {
                return new TheoryData<string, string, string, int, int>
                {
                    { "Receiver", null, "12345678", 2, 8 },
                    { "RECEIVER", null, "12345678", 2, 8 },
                    { "receiver", null, "12345678", 2, 8 },
                    { "Receiver", string.Empty, "12345678", 8, 16 },
                    { "RECEIVER", string.Empty, "12345678", 8, 16 },
                    { "receiver", string.Empty, "12345678", 8, 16 },
                    { "Receiver", "1", "12345678", 2, 16 },
                    { "RECEIVER", "1234567890", "12345678", 2, 16 },
                    { WebHookReceiverMock.ReceiverName, "你好", "12345678", 2, 16 },
                };
            }
        }

        [Fact]
        public async Task EnsureValidCode_Throws_IfNotUsingHttps()
        {
            // Arrange
            Initialize(TestSecret);
            _request.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.EnsureValidCode(_request, TestId));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'WebHookReceiverMock' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
        }

        [Theory]
        [MemberData("InvalidCodeQueries")]
        public async Task EnsureValidCode_Throws_IfNoCodeParameter(string query)
        {
            // Arrange
            Initialize(TestSecret);
            _request.RequestUri = new Uri("https://some.no.ssl.host?" + query);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.EnsureValidCode(_request, TestId));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook verification request must contain a 'code' query parameter.", error.Message);
        }

        [Theory]
        [MemberData("ValidIdData")]
        public async Task EnsureValidCode_Throws_IfWrongCodeParameter(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            _request.RequestUri = new Uri("https://some.no.ssl.host?code=invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.EnsureValidCode(_request, id));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'code' query parameter provided in the HTTP request did not match the expected value.", error.Message);
        }

        [Theory]
        [MemberData("ValidIdData")]
        public void EnsureValidCode_Succeeds_IfRightCodeParameter(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            _request.RequestUri = new Uri("https://some.no.ssl.host?code=12345678901234567890123456789012");

            // Act
            _receiverMock.EnsureValidCode(_request, "Secret");
        }

        [Theory]
        [InlineData("http://example.org")]
        [InlineData("HTTP://example.org")]
        [InlineData("http://local")]
        public async Task EnsureSecureConnection_ThrowsIfNotLocalAndNotHttps(string address)
        {
            // Arrange
            Initialize(TestSecret);
            _request.RequestUri = new Uri(address);

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => _receiverMock.EnsureSecureConnection(_request));

            // Assert
            string expected = string.Format("The WebHook receiver 'WebHookReceiverMock' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.");
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expected, error.Message);
        }

        [Theory]
        [InlineData("12345678", 16, 32)]
        [InlineData("12345678", 2, 4)]
        public async Task GetReceiverConfig_Throws_IfInvalidSecret(string secret, int minLength, int maxLength)
        {
            // Arrange
            Initialize(secret);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.GetReceiverConfig(_request, _receiverMock.Name, TestId, minLength, maxLength));

            // Assert
            string expected = string.Format("Could not find a valid configuration for WebHook receiver 'MockReceiver' and instance '{0}'. The setting must be set to a value between {1} and {2} characters long.", TestId, minLength, maxLength);
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expected, error.Message);
        }

        [Theory]
        [MemberData("ValidConfigData")]
        public async Task GetReceiverConfig_Succeeds_IfValidSecret(string name, string id, string secret, int minLength, int maxLength)
        {
            // Arrange
            Initialize(name, id, secret);

            // Act
            string actual = await _receiverMock.GetReceiverConfig(_request, name, id, minLength, maxLength);

            // Assert
            Assert.Equal(secret, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(10)]
        public async Task GetRequestHeader_Throws_IfHeaderCount_IsNotOne(int headers)
        {
            // Arrange
            Initialize(TestSecret);
            string name = "signature";
            for (int cnt = 0; cnt < headers; cnt++)
            {
                _request.Headers.Add(name, "value");
            }

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => _receiverMock.GetRequestHeader(_request, name));

            // Assert
            string expected = string.Format("Expecting exactly one 'signature' header field in the WebHook request but found {0}. Please ensure that the request contains exactly one 'signature' header field.", headers);
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal(expected, error.Message);
        }

        [Fact]
        public void GetRequestHeader_Succeeds_WithOneHeader()
        {
            // Arrange
            Initialize(TestSecret);
            string expected = "value";
            string name = "signature";
            _request.Headers.Add(name, expected);

            // Act
            string actual = _receiverMock.GetRequestHeader(_request, name);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReadAsJsonAsync_Throws_IfNullBody()
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsJsonAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request entity body cannot be empty.", error.Message);
        }

        [Fact]
        public async Task ReadAsJsonAsync_Throws_IfNotJson()
        {
            // Arrange
            Initialize(TestSecret);
            _request.Content = new StringContent("Hello World", Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsJsonAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
        }

        [Fact]
        public async Task ReadAsJsonAsync_Throws_IfInvalidJson()
        {
            // Arrange
            Initialize(TestSecret);
            _request.Content = new StringContent("I n v a l i d  J S O N", Encoding.UTF8, "application/json");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsJsonAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as valid JSON.", error.Message);
        }

        [Fact]
        public async Task ReadAsJsonAsync_Succeeds_OnValidJson()
        {
            // Arrange
            Initialize(TestSecret);
            _request.Content = new StringContent("{ \"k\": \"v\" }", Encoding.UTF8, "application/json");

            // Act
            JObject actual = await _receiverMock.ReadAsJsonAsync(_request);

            // Assert
            Assert.Equal("v", actual["k"]);
        }

        [Fact]
        public async Task ReadAsXmlAsync_Throws_IfNullBody()
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsXmlAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request entity body cannot be empty.", error.Message);
        }

        [Fact]
        public async Task ReadAsXmlAsync_Throws_IfNotXml()
        {
            // Arrange
            Initialize(TestSecret);
            _request.Content = new StringContent("Hello World", Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsXmlAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as XML.", error.Message);
        }

        [Fact]
        public async Task ReadAsXmlAsync_Throws_IfInvalidXml()
        {
            // Arrange
            Initialize(TestSecret);
            _request.Content = new StringContent("I n v a l i d  X M L", Encoding.UTF8, "application/xml");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsXmlAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as valid XML.", error.Message);
        }

        [Fact]
        public async Task ReadAsXmlAsync_Succeeds_OnValidXml()
        {
            // Arrange
            Initialize(TestSecret);
            _request.Content = new StringContent("<root><k>v</k></root>", Encoding.UTF8, "application/xml");

            // Act
            XElement actual = await _receiverMock.ReadAsXmlAsync(_request);

            // Assert
            Assert.Equal("v", actual.Element("k").Value);
        }

        [Fact]
        public async Task ReadAsFormDataAsync_Throws_IfNullBody()
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsFormDataAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request entity body cannot be empty.", error.Message);
        }

        [Fact]
        public async Task ReadAsFormDataAsync_Throws_IfNotFormData()
        {
            // Arrange
            Initialize(TestSecret);
            _request.Content = new StringContent("Hello World", Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.ReadAsFormDataAsync(_request));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as HTML Form Data.", error.Message);
        }

        [Fact]
        public async Task ReadAsFormDataAsync_Succeeds_OnValidFormData()
        {
            // Arrange
            Initialize(TestSecret);
            _request.Content = new StringContent("k=v", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            NameValueCollection actual = await _receiverMock.ReadAsFormDataAsync(_request);

            // Assert
            Assert.Equal("v", actual["k"]);
        }

        [Fact]
        public async Task CreateBadMethodResponse_CreatesExpectedResponse()
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            HttpResponseMessage actual = _receiverMock.CreateBadMethodResponse(_request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP 'GET' method is not supported by the 'WebHookReceiverMock' WebHook receiver.", error.Message);
        }

        [Fact]
        public async Task CreateBadSignatureResponse_CreatesExpectedResponse()
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            HttpResponseMessage actual = _receiverMock.CreateBadSignatureResponse(_request, "Header");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'Header' header field does not match the value expected by the 'WebHookReceiverMock' receiver. WebHook request is invalid.", error.Message);
        }

        [Fact]
        public async Task ExecuteWebHookAsync_StopsOnFirstResponse()
        {
            // Arrange
            KeyValuePair<Type, object>[] handlers = new KeyValuePair<Type, object>[]
            {
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 10, }),
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 20, }),
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 30, SetResponse = true }),
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 40, }),
            };
            Initialize(TestSecret, handlers);
            object data = new object();

            // Act
            HttpResponseMessage actual = await _receiverMock.ExecuteWebHookAsync(TestId, _context, _request, new[] { "action" }, data);

            // Assert
            Assert.Equal("Order: 30", actual.ReasonPhrase);
            Assert.True(((TestHandler)handlers[0].Value).IsCalled);
            Assert.True(((TestHandler)handlers[1].Value).IsCalled);
            Assert.True(((TestHandler)handlers[2].Value).IsCalled);
            Assert.False(((TestHandler)handlers[3].Value).IsCalled);
        }

        [Fact]
        public async Task ExecuteWebHookAsync_FindsMatchingHandlers()
        {
            // Arrange
            KeyValuePair<Type, object>[] handlers = new KeyValuePair<Type, object>[]
            {
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 10, Receiver = "other" }),
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 20, Receiver = "MockReceiver" }),
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 30, Receiver = "MOCKRECEIVER" }),
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 40, Receiver = null }),
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), new TestHandler { Order = 50, Receiver = "something" }),
            };
            Initialize(TestSecret, handlers);
            object data = new object();

            // Act
            HttpResponseMessage actual = await _receiverMock.ExecuteWebHookAsync(TestId, _context, _request, new[] { "action" }, data);

            // Assert
            Assert.Equal("OK", actual.ReasonPhrase);
            Assert.False(((TestHandler)handlers[0].Value).IsCalled);
            Assert.True(((TestHandler)handlers[1].Value).IsCalled);
            Assert.True(((TestHandler)handlers[2].Value).IsCalled);
            Assert.True(((TestHandler)handlers[3].Value).IsCalled);
            Assert.False(((TestHandler)handlers[4].Value).IsCalled);
        }

        [Fact]
        public async Task ExecuteWebHookAsync_InitializesContext()
        {
            // Arrange
            WebHookHandlerContext actual = null;
            Mock<IWebHookHandler> handlerMock = new Mock<IWebHookHandler>();
            handlerMock.Setup<Task>(h => h.ExecuteAsync(WebHookReceiverMock.ReceiverName, It.IsAny<WebHookHandlerContext>()))
                .Callback<string, WebHookHandlerContext>((rec, con) => actual = con)
                .Returns(Task.FromResult(true))
                .Verifiable();

            KeyValuePair<Type, object>[] handlers = new KeyValuePair<Type, object>[]
            {
                new KeyValuePair<Type, object>(typeof(IWebHookHandler), handlerMock.Object),
            };
            Initialize(TestSecret, handlers);
            object data = new object();
            IEnumerable<string> actions = new[] { "action" };

            // Act
            await _receiverMock.ExecuteWebHookAsync(TestId, _context, _request, actions, data);

            // Assert
            handlerMock.Verify();
            Assert.Equal(TestId, actual.Id);
            Assert.Equal(_request, actual.Request);
            Assert.Equal(_context, actual.RequestContext);
            Assert.Equal(data, actual.Data);
            Assert.Equal(actions, actual.Actions);
        }

        [Theory]
        [MemberData("ByteCompareData")]
        public void SecretEqual_ComparesByteArraysCorrectly(byte[] inputA, byte[] inputB, bool expected)
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            bool actual = WebHookReceiver.SecretEqual(inputA, inputB);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("StringCompareData")]
        public void SecretEqual_ComparesStringsCorrectly(string inputA, string inputB, bool expected)
        {
            // Arrange
            Initialize(TestSecret);

            // Act
            bool actual = WebHookReceiver.SecretEqual(inputA, inputB);

            // Assert
            Assert.Equal(expected, actual);
        }

        public void Initialize(string config, params KeyValuePair<Type, object>[] dependencies)
        {
            Initialize(WebHookReceiverMock.ReceiverName, TestId, config, dependencies);
        }

        public void Initialize(string name, string id, string config, params KeyValuePair<Type, object>[] dependencies)
        {
            _httpConfig = new HttpConfiguration();

            _logger = new Mock<ILogger>().Object;
            _settings = new SettingsDictionary();
            _settings[SecretPrefix + name] = GetConfigValue(id, config);

            _config = new WebHookReceiverConfig(_settings, _logger);

            List<KeyValuePair<Type, object>> deps = new List<KeyValuePair<Type, object>>()
            {
                new KeyValuePair<Type, object>(typeof(IWebHookReceiverConfig), _config),
                new KeyValuePair<Type, object>(typeof(SettingsDictionary), _settings)
            };
            deps.AddRange(dependencies);

            _httpConfig = HttpConfigurationMock.Create(deps);

            _request = new HttpRequestMessage();
            _receiverMock = new WebHookReceiverMock();

            _context = new HttpRequestContext { Configuration = _httpConfig };
            _request.SetRequestContext(_context);
        }

        private static string GetConfigValue(string id, string config)
        {
            return string.IsNullOrEmpty(id) ? config : id + " = " + config;
        }

        private class TestHandler : IWebHookHandler
        {
            public int Order { get; set; }

            public string Receiver { get; set; }

            public bool SetResponse { get; set; }

            public bool IsCalled { get; set; }

            public Task ExecuteAsync(string receiver, WebHookHandlerContext context)
            {
                IsCalled = true;
                if (SetResponse)
                {
                    context.Response = new HttpResponseMessage();
                    context.Response.ReasonPhrase = "Order: " + Order;
                }
                return Task.FromResult(true);
            }
        }
    }
}