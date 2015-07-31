// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestUtilities.Mocks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class TrelloWebHookClientTests
    {
        private const string TestToken = "496dd3ba8b9b42e84a660602a31d683f37f90f049f692a703b07d3e08a9231d5";
        private const string TestAppKey = "7df822c60b2758338b62921c64aded4f";
        private const string TestModelId = "43a28d6a8b46876f44be5589";
        private const string TestDescription = "你好世界";
        private const string TestId = "12b2431e389abdc9c3632516";

        private readonly Uri _receiver;
        private readonly HttpClient _httpClient;
        private readonly HttpMessageHandlerMock _handlerMock;
        private readonly TrelloWebHookClient _trelloClient;

        public TrelloWebHookClientTests()
        {
            _receiver = new Uri("http://localhost/");
            _handlerMock = new HttpMessageHandlerMock();
            _httpClient = new HttpClient(_handlerMock);
            _trelloClient = new TrelloWebHookClient(TestToken, TestAppKey, _httpClient);
        }

        [Fact]
        public async Task CreateAsync_FailsOnRelativeReceiverAddress()
        {
            // Arrange
            Uri relative = new Uri("relative", UriKind.Relative);

            // Act
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(() => _trelloClient.CreateAsync(relative, TestModelId, TestDescription));

            // Assert
            Assert.Contains("The URI for where Trello will send WebHook requests must be an absolute URI. By default this should be of the form 'https://<host>/api/webhooks/incoming/trello'.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_CreatesExpectedRequestAndHandlesSuccess()
        {
            // Arrange
            HttpRequestMessage request = null;
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent("{\"id\":\"12b2431e389abdc9c3632516\",\"description\":\"My Trello WebHook!\",\"idModel\":\"43a28d6a8b46876f44be5589\",\"callbackURL\":\"http://localhost\",\"active\":true}", Encoding.UTF8, "application/json");
            _handlerMock.Handler = (req, counter) =>
            {
                request = req;
                return Task.FromResult(response);
            };
            string expectedTrelloUri = string.Format("https://trello.com/1/tokens/{0}/webhooks/?key={1}", TestToken, TestAppKey);

            // Act
            string actual = await _trelloClient.CreateAsync(_receiver, TestModelId, TestDescription);

            // Assert
            Assert.Equal(expectedTrelloUri, request.RequestUri.AbsoluteUri);

            JObject data = await request.Content.ReadAsAsync<JObject>();
            Assert.Equal(_receiver.AbsoluteUri, data["callbackURL"]);
            Assert.Equal(TestModelId, data["idModel"]);
            Assert.Equal(TestDescription, data["description"]);

            Assert.Equal(TestId, actual);
        }

        [Fact]
        public async Task CreateAsync_ThrowsOnErrorResponse()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent("A WebHook with that callback, model, and token already exists\n", Encoding.UTF8, "text/plain");
            _handlerMock.Handler = (req, counter) => Task.FromResult(response);

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _trelloClient.CreateAsync(_receiver, TestModelId, TestDescription));

            // Assert
            Assert.Equal("Could not create Trello WebHook. Received status code 'BadRequest' and error message 'A WebHook with that callback, model, and token already exists'.", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_CreatesExpectedRequestAndHandlesSuccess()
        {
            // Arrange
            HttpRequestMessage request = null;
            HttpResponseMessage response = new HttpResponseMessage();
            _handlerMock.Handler = (req, counter) =>
            {
                request = req;
                return Task.FromResult(response);
            };
            string expectedTrelloUri = string.Format("https://trello.com/1/webhooks/{0}?key={1}&token={2}", TestId, TestAppKey, TestToken);

            // Act
            bool actual = await _trelloClient.DeleteAsync(TestId);

            // Assert
            Assert.Equal(expectedTrelloUri, request.RequestUri.AbsoluteUri);
            Assert.True(actual);
        }

        [Fact]
        public async Task DeleteAsync_HandlesErrorResponse()
        {
            // Arrange
            HttpRequestMessage request = null;
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _handlerMock.Handler = (req, counter) =>
            {
                request = req;
                return Task.FromResult(response);
            };
            string expectedTrelloUri = string.Format("https://trello.com/1/webhooks/{0}?key={1}&token={2}", TestId, TestAppKey, TestToken);

            // Act
            bool actual = await _trelloClient.DeleteAsync(TestId);

            // Assert
            Assert.Equal(expectedTrelloUri, request.RequestUri.AbsoluteUri);
            Assert.False(actual);
        }
    }
}
