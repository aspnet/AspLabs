// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SlackCoreReceiver;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.FunctionalTest
{
    public class SlackCoreReceiverTest : IClassFixture<WebHookTestFixture<Startup>>
    {
        private readonly HttpClient _client;

        public SlackCoreReceiverTest(WebHookTestFixture<Startup> fixture)
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

        [Theory(Skip = "Flaky test aspnet/WebHooks#314; see also aspnet/WebHooks#318.")]
        [MemberData(nameof(NonPostDataSet))]
        public async Task WebHookAction_NonPost_IsNotAllowed(HttpMethod method)
        {
            // Arrange
            var expectedErrorMessage = $"The 'slack' WebHook receiver does not support the HTTP '{method.Method}' " +
                "method.";
            var request = new HttpRequestMessage(method, "/api/webhooks/incoming/slack");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }
    }
}
