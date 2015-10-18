// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramWebHookClientTests
    {
        private const string TestId = "";
        private const string TestSubscriptions = "{ \"meta\": { \"code\": 200 }, \"data\": [ { \"id\": \"1\", \"type\": \"subscribe\", \"object\": \"user\", \"aspect\": \"media\", \"callback_url\": \"http://your-callback.com/url/\" }, { \"id\": \"2\", \"type\": \"subscription\", \"object\": \"location\", \"object_id\": \"2345\", \"aspect\": \"media\", \"callback_url\": \"http://your-callback.com/url/\" } ] }";
        private const string TestClientId = "41225b0f627e4c31a442f1ebf55e4a6d";
        private const string TestClientSecret = "a0913d87aba24e689266ad8ecbd3832e";
        private const string TestSubAddress = "https://api.instagram.com/v1/subscriptions";
        private const string TestCallback = "https://www.exmample.org/callback";

        private readonly HttpConfiguration _httpConfig;
        private readonly HttpClient _httpClient;
        private readonly HttpMessageHandlerMock _handlerMock;
        private readonly Mock<InstagramWebHookClient> _clientMock;
        private readonly InstagramWebHookClient _client;
        private readonly Uri _callback;

        public InstagramWebHookClientTests()
        {
            _httpConfig = new HttpConfiguration();
            _handlerMock = new HttpMessageHandlerMock();
            _httpClient = new HttpClient(_handlerMock);
            _clientMock = new Mock<InstagramWebHookClient>(_httpConfig, _httpClient) { CallBase = true };
            _client = _clientMock.Object;
            _callback = new Uri(TestCallback);
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

        [MemberData("ValidIdData")]
        public async Task GetAllSubscriptionsAsync_Throws_OnError(string id)
        {
            // Arrange
            Initialize(id);
            _handlerMock.Handler = (req, reqId) =>
            {
                HttpResponseMessage rsp = new HttpResponseMessage();
                rsp.Content = new StringContent(TestSubscriptions, Encoding.UTF8, "application/json");
                return Task.FromResult(rsp);
            };

            // Act
            Collection<InstagramSubscription> actual = await _client.GetAllSubscriptionsAsync(id);

            // Assert
            Assert.Equal(2, actual.Count);
            Assert.Equal("1", actual[0].Id);
            Assert.Equal("user", actual[0].Object);
            Assert.Equal("2", actual[1].Id);
            Assert.Equal("location", actual[1].Object);
        }

        [Fact]
        public async Task SubscribeAsync_FailsOnRelativeReceiverAddress()
        {
            // Arrange
            Initialize(TestId);
            Uri relative = new Uri("relative", UriKind.Relative);

            // Act
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(() => _client.SubscribeAsync(TestId, relative));

            // Assert
            Assert.Contains("The URI for where Instagram will send WebHook requests must be an absolute URI. By default this should be of the form 'https://<host>/api/webhooks/incoming/instagram'.", ex.Message);
        }

        [Fact]
        public async Task SubscribeAsync_CreatesUserSubscription()
        {
            // Arrange
            Initialize(TestId);
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent("{ \"meta\": { \"code\": 200 }, \"data\": { \"id\": \"1\", \"type\": \"subscribe\", \"object\": \"user\", \"aspect\": \"media\", \"callback_url\": \"" + TestCallback + "\" } }", Encoding.UTF8, "application/json");
            _handlerMock.Handler = async (req, counter) =>
            {
                MultipartFormDataContent content = await ValidateCoreSubscriptionRequest(req);
                await ValidateSubscriptionContent(content, 0, "object", "user");
                await ValidateSubscriptionContent(content, 1, "aspect", "media");
                return response;
            };

            // Act
            InstagramSubscription actual = await _client.SubscribeAsync(TestId, _callback);

            // Assert
            Assert.Equal("1", actual.Id);
            Assert.Equal("user", actual.Object);
            Assert.Equal(TestCallback, actual.Callback.AbsoluteUri);
        }

        [Fact]
        public async Task SubscribeAsync_CreatesTagSubscription()
        {
            // Arrange
            Initialize(TestId);
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent("{ \"meta\": { \"code\": 200 }, \"data\": { \"id\": \"1\", \"type\": \"subscribe\", \"object\": \"tag\", \"object_id\": \"12345\", \"aspect\": \"media\", \"callback_url\": \"" + TestCallback + "\" } }", Encoding.UTF8, "application/json");
            _handlerMock.Handler = async (req, counter) =>
            {
                MultipartFormDataContent content = await ValidateCoreSubscriptionRequest(req);
                await ValidateSubscriptionContent(content, 0, "object", "tag");
                await ValidateSubscriptionContent(content, 1, "aspect", "media");
                await ValidateSubscriptionContent(content, 2, "object_id", "12345");
                return response;
            };

            // Act
            InstagramSubscription actual = await _client.SubscribeAsync(TestId, _callback, "12345");

            // Assert
            Assert.Equal("1", actual.Id);
            Assert.Equal("tag", actual.Object);
            Assert.Equal(TestCallback, actual.Callback.AbsoluteUri);
        }

        [Fact]
        public async Task SubscribeAsync_CreatesGeoSubscription()
        {
            // Arrange
            Initialize(TestId);
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent("{ \"meta\": { \"code\": 200 }, \"data\": { \"id\": \"1\", \"type\": \"subscribe\", \"object\": \"geography\", \"object_id\": \"12345\", \"aspect\": \"media\", \"callback_url\": \"" + TestCallback + "\" } }", Encoding.UTF8, "application/json");
            _handlerMock.Handler = async (req, counter) =>
            {
                MultipartFormDataContent content = await ValidateCoreSubscriptionRequest(req);
                await ValidateSubscriptionContent(content, 0, "object", "geography");
                await ValidateSubscriptionContent(content, 1, "aspect", "media");
                await ValidateSubscriptionContent(content, 2, "lat", "1.2345");
                await ValidateSubscriptionContent(content, 3, "lng", "2.2345");
                await ValidateSubscriptionContent(content, 4, "radius", "1000");
                return response;
            };

            // Act
            InstagramSubscription actual = await _client.SubscribeAsync(TestId, _callback, 1.2345, 2.2345, 1000);

            // Assert
            Assert.Equal("1", actual.Id);
            Assert.Equal("geography", actual.Object);
            Assert.Equal(TestCallback, actual.Callback.AbsoluteUri);
        }

        [Fact]
        public async Task CreateAsync_ThrowsOnErrorResponse()
        {
            // Arrange
            Initialize(TestId);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent("{\"meta\":{\"error_type\":\"APISubscriptionError\",\"code\":400,\"error_message\":\"Invalid format\"}}", Encoding.UTF8, "text/plain");
            _handlerMock.Handler = (req, counter) => Task.FromResult(response);

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.SubscribeAsync(TestId, _callback));

            // Assert
            Assert.StartsWith("Could not create Instagram subscription", ex.Message);
        }

        private async Task<MultipartFormDataContent> ValidateCoreSubscriptionRequest(HttpRequestMessage req)
        {
            Assert.Equal(TestSubAddress, req.RequestUri.AbsoluteUri);
            MultipartFormDataContent content = (MultipartFormDataContent)req.Content;

            int last = content.Count() - 1;
            await ValidateSubscriptionContent(content, last - 2, "client_id", TestClientId);
            await ValidateSubscriptionContent(content, last - 1, "client_secret", TestClientSecret);
            await ValidateSubscriptionContent(content, last, "callback_url", TestCallback);

            return content;
        }

        private async Task ValidateSubscriptionContent(MultipartFormDataContent content, int index, string name, string value)
        {
            StringContent parameter = (StringContent)content.ElementAt(index);
            string actual = await parameter.ReadAsStringAsync();

            Assert.Equal(value, actual);
            Assert.Null(parameter.Headers.ContentType);
            ContentDispositionHeaderValue cd = parameter.Headers.ContentDisposition;
            Assert.Equal(name, cd.Name);
        }

        private void Initialize(string id)
        {
            _clientMock.Protected()
                .Setup<Task<Tuple<string, string>>>("GetClientConfig", id)
                .ReturnsAsync(Tuple.Create(TestClientId, TestClientSecret))
                .Verifiable();
        }
    }
}
