// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MailChimpCoreReceiver;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.FunctionalTest
{
    public class MailChimpCoreReceiverTest : IClassFixture<WebHookTestFixture<Startup>>
    {
        private readonly HttpClient _client;

        public MailChimpCoreReceiverTest(WebHookTestFixture<Startup> fixture)
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

        public static TheoryData<HttpMethod> NonGetOrPostDataSet
        {
            get
            {
                return new TheoryData<HttpMethod>
                {
                    HttpMethod.Head,
                    HttpMethod.Put,
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonGetOrPostDataSet))]
        public async Task WebHookAction_NonGetOrPost_IsNotAllowed(HttpMethod method)
        {
            // Arrange
            var expectedErrorMessage = "The 'mailchimp' WebHook receiver does not support the HTTP " +
                $"'{method.Method}' method.";
            var request = new HttpRequestMessage(
                method,
                "/api/webhooks/incoming/mailchimp?code=01234567890123456789012345678901");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }

        [Fact]
        public async Task WebHookAction_NoCode_IsBadRequest()
        {
            // Arrange
            var expectedErrorMessage = "A 'mailchimp' WebHook request must contain a 'code' query parameter.";
            var content = new StringContent(string.Empty);

            // Act
            var response = await _client.PostAsync("/api/webhooks/incoming/mailchimp", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }

        [Fact]
        public async Task WebHookAction_WrongCode_IsBadRequest()
        {
            // Arrange
            var expectedErrorMessage = "The 'code' query parameter provided in the HTTP request did not match the " +
                "expected value.";
            var content = new StringContent(string.Empty);

            // Act
            var response = await _client.PostAsync(
                // One changed character in code query parameter.
                "/api/webhooks/incoming/mailchimp?code=01234567890123456789012345678902",
                content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }

        [Fact]
        public async Task WebHookAction_Get_Succeeds()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/webhooks/incoming/mailchimp?code=01234567890123456789012345678901&challenge=012345678901234");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseText);
        }

        [Fact]
        public async Task WebHookAction_NoBody_IsBadRequest()
        {
            // Arrange
            var expectedErrorMessage = "The 'mailchimp' WebHook receiver does not support an empty request body.";
            var content = new StringContent(string.Empty);

            // Act
            var response = await _client.PostAsync(
                "/api/webhooks/incoming/mailchimp?code=01234567890123456789012345678901",
                content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }
    }
}
