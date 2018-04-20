// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubCoreReceiver;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.FunctionalTest
{
    public class GitHubCoreReceiverTest : IClassFixture<WebHookTestFixture<Startup>>
    {
        private readonly HttpClient _client;

        public GitHubCoreReceiverTest(WebHookTestFixture<Startup> fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task HomePage_IsNotFound()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task WebHookAction_NoEventHeader_IsNotFound()
        {
            // Arrange
            var content = new StringContent(string.Empty);
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/incoming/github")
            {
                Content = content,
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();

            // This requirement is enforced in a constraint. Therefore, response is empty.
            Assert.Empty(responseText);
        }

        public static TheoryData<HttpMethod> NonPostDataSet
        {
            get
            {
                return new TheoryData<HttpMethod>
                {
                    HttpMethod.Get,
                    HttpMethod.Head,
                    HttpMethod.Put,
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonPostDataSet))]
        public async Task WebHookAction_NonPost_IsNotAllowed(HttpMethod method)
        {
            // Arrange
            var expectedErrorMessage = $"The 'github' WebHook receiver does not support the HTTP '{method.Method}' " +
                "method.";
            var request = new HttpRequestMessage(method, "/api/webhooks/incoming/github")
            {
                Headers =
                {
                    { GitHubConstants.EventHeaderName, "push" },
                },
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }
    }
}
