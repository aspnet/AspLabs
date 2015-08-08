// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.TestUtilities;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class PusherWebHookReceiverTests
    {
        private const string TestReceiver = "Test";
        private const string TestKey = "9876543210";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestKeySecret = TestKey + "_" + TestSecret;
        private const string TestContent = "{ \"time_ms\": 1327078148132, \"events\": [ { \"name\": \"event_name\", \"some\": \"data\" } ] }";

        private HttpConfiguration _config;
        private SettingsDictionary _settings;
        private HttpRequestContext _context;
        private Mock<PusherWebHookReceiver> _receiverMock;
        private HttpRequestMessage _postRequest;
        private string _testSignature;

        public PusherWebHookReceiverTests()
        {
            _settings = new SettingsDictionary();
            _settings["MS_WebHookReceiverSecret_Pusher"] = TestKeySecret;

            _config = HttpConfigurationMock.Create(new Dictionary<Type, object> { { typeof(SettingsDictionary), _settings } });
            _context = new HttpRequestContext { Configuration = _config };

            _receiverMock = new Mock<PusherWebHookReceiver> { CallBase = true };

            _postRequest = new HttpRequestMessage(HttpMethod.Post, "https://some.ssl.host");
            _postRequest.SetRequestContext(_context);
            _postRequest.Headers.Add(PusherWebHookReceiver.KeyHeaderName, TestKey);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");

            byte[] secret = Encoding.UTF8.GetBytes(TestSecret);
            using (var hasher = new HMACSHA256(secret))
            {
                byte[] data = Encoding.UTF8.GetBytes(TestContent);
                byte[] testHash = hasher.ComputeHash(data);
                _testSignature = EncodingUtilities.ToHex(testHash);
            }
        }

        public static TheoryDataCollection<string, long, string[]> PusherData
        {
            get
            {
                return new TheoryDataCollection<string, long, string[]>
                {
                    { "{\"time_ms\":1437252687875,\"events\":[{\"channel\":\"my_channel\",\"name\":\"channel_vacated\"}]}", 1437252687875, new[] { "channel_vacated" } },
                    { "{\"time_ms\":1437252692478,\"events\":[{\"channel\":\"my_channel\",\"name\":\"channel_vacated\"},{\"channel\":\"my_channel\",\"name\":\"你好世界\"},]}", 1437252692478, new[] { "channel_vacated", "你好世界" } },
                    { "{\"time_ms\":1437252687875,\"events\":[{\"channel\":\"my_channel\",\"name\":\"channel_vacated\"},{\"channel\":\"my_channel\"}]}", 1437252687875, new[] { "channel_vacated" } },
                    { "{\"time_ms\":1437252687875,\"events\":[{\"channel\":\"my_channel\"},{\"channel\":\"my_channel\",\"name\":\"channel_vacated\"}]}", 1437252687875, new[] { "channel_vacated" } },
                };
            }
        }

        public static TheoryDataCollection<string, IDictionary<string, string>> ValidSecretData
        {
            get
            {
                return new TheoryDataCollection<string, IDictionary<string, string>>
                {
                    { "key1_secret1", new Dictionary<string, string> { { "key1", "secret1" } } },
                    { "你好_secret1,世界_secret2", new Dictionary<string, string> { { "你好", "secret1" }, { "世界", "secret2" } } },
                    { "key1_你好, key2_世界", new Dictionary<string, string> { { "key1", "你好" }, { "key2", "世界" } } },
                    { "key1_secret1 ,key2_secret2", new Dictionary<string, string> { { "key1", "secret1" }, { "key2", "secret2" } } },
                    { "key1_secret1 , key2_secret2", new Dictionary<string, string> { { "key1", "secret1" }, { "key2", "secret2" } } },
                    { "key1 _secret1, key2_ secret2, key3 _ secret3", new Dictionary<string, string> { { "key1", "secret1" }, { "key2", "secret2" }, { "key3", "secret3" } } },
                    { "key1__secret1,,, key2__secret2,,, key3___secret3", new Dictionary<string, string> { { "key1", "secret1" }, { "key2", "secret2" }, { "key3", "secret3" } } },
                };
            }
        }

        public static TheoryDataCollection<string> InvalidSecretData
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                   "        _",
                   "        key",
                   "        key_",
                   "        _secret",
                   "        key_key_secret",
                   "        key_key_secret_secret",
                };
            }
        }

        [Theory]
        [MemberData("PusherData")]
        public async Task GetPusherNotification_ExtractsData(string data, long createdAt, IEnumerable<string> actions)
        {
            ReceiverMock mock = new ReceiverMock();
            _postRequest.Content = new StringContent(data, Encoding.UTF8, "application/json");

            // Act
            PusherNotification actual = await mock.GetPusherNotification(_postRequest);

            // Assert
            Assert.Equal(createdAt, actual.CreatedAt);
            Assert.Equal(actions, actual.Events.Keys);
        }

        [Theory]
        [MemberData("ValidSecretData")]
        public void GetSecretLookupTable_BuildsLookupTable(string secret, IDictionary<string, string> expected)
        {
            // Arrange
            ReceiverMock mock = new ReceiverMock();
            _settings["MS_WebHookReceiverSecret_Pusher"] = secret;

            // Act
            IDictionary<string, string> actual = mock.GetSecretLookupTable(_postRequest);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetSecretLookupTable_ReturnsSameInstance()
        {
            // Arrange
            ReceiverMock mock = new ReceiverMock();

            // Act
            IDictionary<string, string> actual1 = mock.GetSecretLookupTable(_postRequest);
            IDictionary<string, string> actual2 = mock.GetSecretLookupTable(_postRequest);

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Theory]
        [MemberData("InvalidSecretData")]
        public async Task GetSecretLookupTable_Throws_IfInvalidSecret(string invalid)
        {
            // Arrange
            ReceiverMock mock = new ReceiverMock();
            _settings["MS_WebHookReceiverSecret_Pusher"] = invalid;

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => mock.GetSecretLookupTable(_postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The application setting 'MS_WebHookReceiverSecret_Pusher' must have a comma separated list of one or more values of the form '<appKey>_<appSecret>'.", error.Message);
        }

        [Fact]
        public async Task GetSecretLookupTable_Throws_IfNoSecrets()
        {
            // Arrange
            ReceiverMock mock = new ReceiverMock();
            _settings["MS_WebHookReceiverSecret_Pusher"] = "         ";

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => mock.GetSecretLookupTable(_postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Did not find any applications settings of the form 'MS_WebHookReceiverSecret_Pusher'. To receive WebHooks from the 'PusherWebHookReceiver' receiver, please add corresponding applications settings.", error.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasNoSignatureHeader()
        {
            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Pusher-Signature' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'X-Pusher-Signature' header field.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, "value2");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Pusher-Signature' header field in the WebHook request but found 2. Please ensure that the request contains exactly one 'X-Pusher-Signature' header field.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeaderEncoding()
        {
            // Arrange
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, "你好世界");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'X-Pusher-Signature' header value is invalid. It must be a valid hex-encoded string.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnError_IfPostHasInvalidSignature()
        {
            // Arrange
            string invalid = EncodingUtilities.ToHex(Encoding.UTF8.GetBytes("你好世界"));
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, invalid);

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'X-Pusher-Signature' header field does not match the value expected by the 'PusherWebHookReceiverProxy' receiver. WebHook request is invalid.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, _testSignature);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoKeyHeader()
        {
            // Arrange
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, _testSignature);
            _postRequest.Headers.Remove(PusherWebHookReceiver.KeyHeaderName);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Pusher-Key' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'X-Pusher-Key' header field.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasInvalidKeyHeader()
        {
            // Arrange
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, _testSignature);
            _postRequest.Headers.Remove(PusherWebHookReceiver.KeyHeaderName);
            _postRequest.Headers.Add(PusherWebHookReceiver.KeyHeaderName, "invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP header 'X-Pusher-Key' value of 'invalid' is not recognized as a valid application key. Please ensure that the right application keys and secrets have been configured.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            List<string> actions = new List<string> { "event_name" };
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, _testSignature);
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

        private class ReceiverMock : PusherWebHookReceiver
        {
            public new Task<PusherNotification> GetPusherNotification(HttpRequestMessage request)
            {
                return base.GetPusherNotification(request);
            }

            public new IDictionary<string, string> GetSecretLookupTable(HttpRequestMessage request)
            {
                return base.GetSecretLookupTable(request);
            }
        }
    }
}
