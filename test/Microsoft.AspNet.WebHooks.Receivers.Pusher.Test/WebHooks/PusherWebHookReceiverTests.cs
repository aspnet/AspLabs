// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class PusherWebHookReceiverTests : WebHookReceiverTestsBase<PusherWebHookReceiver>
    {
        private const string TestContent = "{ \"time_ms\": 1327078148132, \"events\": [ { \"name\": \"event_name\", \"some\": \"data\" } ] }";
        private const string TestId = "";
        private const string TestKey = "9876543210";
        private const string TestConfig = "12345678901234567890123456789012";
        private const string TestSecret = TestKey + "_" + TestConfig;

        private HttpRequestMessage _postRequest;
        private string _testSignature;

        public PusherWebHookReceiverTests()
        {
            byte[] secret = Encoding.UTF8.GetBytes(TestConfig);
            using (var hasher = new HMACSHA256(secret))
            {
                byte[] data = Encoding.UTF8.GetBytes(TestContent);
                byte[] testHash = hasher.ComputeHash(data);
                _testSignature = EncodingUtilities.ToHex(testHash);
            }
        }

        public static TheoryData<string, string[]> PusherData
        {
            get
            {
                return new TheoryData<string, string[]>
                {
                    { "{\"time_ms\":1437252687875,\"events\":[{\"channel\":\"my_channel\",\"name\":\"channel_vacated\"}]}", new[] { "channel_vacated" } },
                    { "{\"time_ms\":1437252692478,\"events\":[{\"channel\":\"my_channel\",\"name\":\"channel_vacated\"},{\"channel\":\"my_channel\",\"name\":\"你好世界\"},]}", new[] { "channel_vacated", "你好世界" } },
                    { "{\"time_ms\":1437252687875,\"events\":[{\"channel\":\"my_channel\",\"name\":\"channel_vacated\"},{\"channel\":\"my_channel\"}]}", new[] { "channel_vacated" } },
                    { "{\"time_ms\":1437252687875,\"events\":[{\"channel\":\"my_channel\"},{\"channel\":\"my_channel\",\"name\":\"channel_vacated\"}]}", new[] { "channel_vacated" } },
                };
            }
        }

        public static TheoryData<string> InvalidPusherData
        {
            get
            {
                return new TheoryData<string>
                {
                   "{ \"time_ms\": 1437252687875, \"events\": \"i n v a l i d\" }",
                   "{ \"time_ms\": 1437252687875, \"events\": [ { \"name\": { } } ] }",
                };
            }
        }

        public static TheoryData<string, string, IDictionary<string, string>> ValidSecretData
        {
            get
            {
                return new TheoryData<string, string, IDictionary<string, string>>
                {
                    { string.Empty, "key1_secret1", new Dictionary<string, string> { { "key1", "secret1" } } },
                    { "id", "你好_secret1;世界_secret2", new Dictionary<string, string> { { "你好", "secret1" }, { "世界", "secret2" } } },
                    { "你好", "key1_你好; key2_世界", new Dictionary<string, string> { { "key1", "你好" }, { "key2", "世界" } } },
                    { "1", "key1_secret1 ;key2_secret2", new Dictionary<string, string> { { "key1", "secret1" }, { "key2", "secret2" } } },
                    { "1234567890", "key1_secret1 ; key2_secret2", new Dictionary<string, string> { { "key1", "secret1" }, { "key2", "secret2" } } },
                    { "1234567890", "key1 _secret1; key2_ secret2; key3 _ secret3", new Dictionary<string, string> { { "key1", "secret1" }, { "key2", "secret2" }, { "key3", "secret3" } } },
                    { "1234567890", "key1__secret1;;; key2__secret2;;; key3___secret3", new Dictionary<string, string> { { "key1", "secret1" }, { "key2", "secret2" }, { "key3", "secret3" } } },
                };
            }
        }

        public static TheoryData<string, string> InvalidSecretData
        {
            get
            {
                return new TheoryData<string, string>
                {
                   { string.Empty, "12345678901234567890123456789012_" },
                   { "id", "12345678901234567890123456789012key" },
                   { "你好", "12345678901234567890123456789012key_" },
                   { "1", "_12345678901234567890123456789012secret" },
                   { "1234567890", "12345678901234567890123456789012key_key_secret" },
                   { "1234567890", "12345678901234567890123456789012key_key_secret_secret" },
                };
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new PusherWebHookReceiver();
            string expected = "pusher";

            // Act
            string actual1 = rec.Name;
            string actual2 = PusherWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
        }

        [Theory]
        [MemberData(nameof(InvalidPusherData))]
        public async Task GetActions_Throws_IfInvalidData(string invalid)
        {
            // Arrange
            Initialize(TestSecret);
            PusherReceiverMock mock = new PusherReceiverMock();
            JObject data = JObject.Parse(invalid);

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => mock.GetActions(_postRequest, data));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.StartsWith("Could not parse Pusher WebHook data: ", error.Message);
        }

        [Theory]
        [MemberData(nameof(PusherData))]
        public void GetActions_ExtractsActions(string valid, IEnumerable<string> actions)
        {
            // Arrange
            Initialize(TestSecret);
            PusherReceiverMock mock = new PusherReceiverMock();
            JObject data = JObject.Parse(valid);

            // Act
            IEnumerable<string> actual = mock.GetActions(_postRequest, data);

            // Assert
            Assert.Equal(actions, actual);
        }

        [Theory]
        [MemberData(nameof(ValidSecretData))]
        public async Task GetSecretLookupTable_BuildsLookupTable(string id, string secret, IDictionary<string, string> expected)
        {
            // Arrange
            Initialize(GetConfigValue(id, secret));
            PusherReceiverMock mock = new PusherReceiverMock();

            // Act
            IDictionary<string, string> actual = await mock.GetSecretLookupTable(id, _postRequest);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetSecretLookupTable_ReturnsSameInstance()
        {
            // Arrange
            Initialize(TestSecret);
            PusherReceiverMock mock = new PusherReceiverMock();

            // Act
            IDictionary<string, string> actual1 = await mock.GetSecretLookupTable(TestId, _postRequest);
            IDictionary<string, string> actual2 = await mock.GetSecretLookupTable(TestId, _postRequest);

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Theory]
        [MemberData(nameof(InvalidSecretData))]
        public async Task GetSecretLookupTable_Throws_IfInvalidSecret(string id, string invalid)
        {
            // Arrange
            PusherReceiverMock mock = new PusherReceiverMock();
            Initialize(GetConfigValue(id, invalid));

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => mock.GetSecretLookupTable(id, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The application setting for Pusher must be a comma-separated list of segments of the form '<appKey1>_<appSecret1>; <appKey2>_<appSecret2>'.", error.Message);
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task GetSecretLookupTable_Throws_IfNoSecrets(string id)
        {
            // Arrange
            Initialize(new string(' ', 32));
            PusherReceiverMock mock = new PusherReceiverMock();

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => mock.GetSecretLookupTable(id, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            string expected = string.Format(CultureInfo.CurrentCulture, "Could not find a valid configuration for WebHook receiver 'pusher' and instance '{0}'. The setting must be set to a value between 8 and 128 characters long.", id);
            Assert.Equal(expected, error.Message);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasNoSignatureHeader()
        {
            // Act
            Initialize(TestSecret);
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Pusher-Signature' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'X-Pusher-Signature' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, "value1");
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, "value2");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Pusher-Signature' header field in the WebHook request but found 2. Please ensure that the request contains exactly one 'X-Pusher-Signature' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostHasInvalidSignatureHeaderEncoding()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, "你好世界");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'X-Pusher-Signature' header value is invalid. It must be a valid hex-encoded string.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnError_IfPostHasInvalidSignature()
        {
            // Arrange
            Initialize(TestSecret);
            string invalid = EncodingUtilities.ToHex(Encoding.UTF8.GetBytes("你好世界"));
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, invalid);

            // Act
            HttpResponseMessage actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook signature provided by the 'X-Pusher-Signature' header field does not match the value expected by the 'PusherWebHookReceiverProxy' receiver. WebHook request is invalid.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotJson()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, _testSignature);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "text/plain");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as JSON.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoKeyHeader()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, _testSignature);
            _postRequest.Headers.Remove(PusherWebHookReceiver.KeyHeaderName);

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Expecting exactly one 'X-Pusher-Key' header field in the WebHook request but found 0. Please ensure that the request contains exactly one 'X-Pusher-Key' header field.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasInvalidKeyHeader()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, _testSignature);
            _postRequest.Headers.Remove(PusherWebHookReceiver.KeyHeaderName);
            _postRequest.Headers.Add(PusherWebHookReceiver.KeyHeaderName, "invalid");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP header 'X-Pusher-Key' value of 'invalid' is not recognized as a valid application key. Please ensure that the right application keys and secrets have been configured.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            List<string> actions = new List<string> { "event_name" };
            _postRequest.Headers.Add(PusherWebHookReceiver.SignatureHeaderName, _testSignature);
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

            _postRequest = new HttpRequestMessage(HttpMethod.Post, "https://some.ssl.host");
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Headers.Add(PusherWebHookReceiver.KeyHeaderName, TestKey);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/json");
        }

        private class PusherReceiverMock : PusherWebHookReceiver
        {
            public new IEnumerable<string> GetActions(HttpRequestMessage request, JObject data)
            {
                return base.GetActions(request, data);
            }

            public new Task<IDictionary<string, string>> GetSecretLookupTable(string id, HttpRequestMessage request)
            {
                return base.GetSecretLookupTable(id, request);
            }
        }
    }
}
