// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BitbucketStronglyTypedCoreReceiver;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.FunctionalTest
{
    public class BitbucketStronglyTypedCoreReceiverTest : IClassFixture<WebHookTestFixture<Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebHookTestFixture<Startup> _fixture;

        public BitbucketStronglyTypedCoreReceiverTest(WebHookTestFixture<Startup> fixture)
        {
            _client = fixture.CreateClient();
            _fixture = fixture;
        }

        [Fact]
        public async Task HomePage_IsNotFound()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(Skip = "Flaky test aspnet/WebHooks#315; see also aspnet/WebHooks#318.")]
        public async Task WebHookAction_NoEventHeader_IsNotFound()
        {
            // Arrange
            var content = new StringContent(string.Empty);
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/webhooks/incoming/bitbucket?code=01234567890123456789012345678901")
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

        [Theory(Skip = "Flaky test aspnet/WebHooks#314; see also aspnet/WebHooks#318.")]
        [MemberData(nameof(NonPostDataSet))]
        public async Task WebHookAction_NonPost_IsNotAllowed(HttpMethod method)
        {
            // Arrange
            var expectedErrorMessage = "The 'bitbucket' WebHook receiver does not support the HTTP " +
                $"'{method.Method}' method.";
            var request = new HttpRequestMessage(
                method,
                "/api/webhooks/incoming/bitbucket?code=01234567890123456789012345678901")
            {
                Headers =
                {
                    { BitbucketConstants.EventHeaderName, "bitbucket:push" },
                    { BitbucketConstants.WebHookIdHeaderName, "01234" },
                },
            };

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
            var expectedErrorMessage = "A 'bitbucket' WebHook request must contain a 'code' query parameter.";
            var content = new StringContent(string.Empty);
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/incoming/bitbucket")
            {
                Content = content,
                Headers =
                {
                    { BitbucketConstants.EventHeaderName, "bitbucket:push" },
                },
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }

        [Fact(Skip = "Flaky test aspnet/WebHooks#316; see also aspnet/WebHooks#318.")]
        public async Task WebHookAction_WrongCode_IsBadRequest()
        {
            // Arrange
            var expectedErrorMessage = "The 'code' query parameter provided in the HTTP request did not match the " +
                "expected value.";
            var content = new StringContent(string.Empty);
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                // One changed character in code query parameter.
                "/api/webhooks/incoming/bitbucket?code=01234567890123456789012345678902")
            {
                Content = content,
                Headers =
                {
                    { BitbucketConstants.EventHeaderName, "bitbucket:push" },
                },
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }

        [Fact(Skip = "Flaky test aspnet/WebHooks#317; see also aspnet/WebHooks#318.")]
        public async Task WebHookAction_NoUUID_IsBadRequest()
        {
            // Arrange
            var expectedErrorMessage = "A 'bitbucket' WebHook request must contain a " +
                $"'{BitbucketConstants.WebHookIdHeaderName}' HTTP header.";
            var content = new StringContent(string.Empty);
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                // One changed character in code query parameter.
                "/api/webhooks/incoming/bitbucket?code=01234567890123456789012345678901")
            {
                Content = content,
                Headers =
                {
                    { BitbucketConstants.EventHeaderName, "bitbucket:push" },
                },
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }

        [Fact(Skip = "Flaky test aspnet/WebHooks#313; see also aspnet/WebHooks#318.")]
        public async Task WebHookAction_NoBody_IsBadRequest()
        {
            // Arrange
            var expectedErrorMessage = "The 'bitbucket' WebHook receiver does not support an empty request body.";
            var content = new StringContent(string.Empty);
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/webhooks/incoming/bitbucket?code=01234567890123456789012345678901")
            {
                Content = content,
                Headers =
                {
                    { BitbucketConstants.EventHeaderName, "bitbucket:push" },
                    { BitbucketConstants.WebHookIdHeaderName, "01234" },
                },
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }

        [Fact]
        public async Task WebHookAction_WithBody_Succeeds()
        {
            // Arrange
            var fixture = _fixture.WithTestLogger(out var testSink);
            var client = fixture.CreateClient();

            var path = Path.Combine("Resources", "RequestBodies", "Bitbucket.json");
            var stream = await ResourceFile.GetResourceStreamAsync(path, normalizeLineEndings: true);
            var content = new StreamContent(stream)
            {
                Headers =
                {
                    { HeaderNames.ContentLength, stream.Length.ToString() },
                    { HeaderNames.ContentType, "text/json" },
                },
            };
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/webhooks/incoming/bitbucket?code=01234567890123456789012345678901")
            {
                Content = content,
                Headers =
                {
                    { BitbucketConstants.EventHeaderName, "repo:push" },
                    { BitbucketConstants.WebHookIdHeaderName, "01234" },
                },
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseText);
        }
    }
}
