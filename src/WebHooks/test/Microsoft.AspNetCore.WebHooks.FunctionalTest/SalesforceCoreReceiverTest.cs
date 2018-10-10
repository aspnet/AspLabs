// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using SalesforceCoreReceiver;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.FunctionalTest
{
    public class SalesforceCoreReceiverTest : IClassFixture<WebHookTestFixture<Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebHookTestFixture<Startup> _fixture;

        public SalesforceCoreReceiverTest(WebHookTestFixture<Startup> fixture)
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
            var expectedErrorMessage = "The 'salesforce' WebHook receiver does not support the HTTP " +
                $"'{method.Method}' method.";
            var request = new HttpRequestMessage(method, "/api/webhooks/incoming/salesforce");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMessage, responseText);
        }

        [Fact]
        public async Task WebHookAction_WithBody_Succeeds()
        {
            // Arrange
            var fixture = _fixture.WithTestLogger(out var testSink);
            var client = fixture.CreateClient();

            var path = Path.Combine("Resources", "RequestBodies", "Salesforce.xml");
            var stream = await ResourceFile.GetResourceStreamAsync(path, normalizeLineEndings: true);
            var content = new StreamContent(stream)
            {
                Headers =
                {
                    { HeaderNames.ContentLength, stream.Length.ToString() },
                    { HeaderNames.ContentType, "text/xml" },
                },
            };

            path = Path.Combine("Resources", "ResponseBodies", "Salesforce.xml");
            var expectedResponseText = await ResourceFile.GetResourceAsStringAsync(path, normalizeLineEndings: false);

            // Act
            var response = await client.PostAsync("/api/webhooks/incoming/salesforce", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponseText, responseText, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async Task WebHookAction_WithNoOrganizationId_IsBadRequest()
        {
            // Arrange
            var expectedFaultString = "<faultstring>The HTTP request body did not contain a required " +
                "'/*[local-name()='Body']/*[local-name()='notifications']/*[local-name()='OrganizationId']' element." +
                "</faultstring>";

            var fixture = _fixture.WithTestLogger(out var testSink);
            var client = fixture.CreateClient();

            var path = Path.Combine("Resources", "RequestBodies", "Salesforce.Empty.xml");
            var stream = await ResourceFile.GetResourceStreamAsync(path, normalizeLineEndings: true);
            var content = new StreamContent(stream)
            {
                Headers =
                {
                    { HeaderNames.ContentLength, stream.Length.ToString() },
                    { HeaderNames.ContentType, "text/xml" },
                },
            };

            // Act
            var response = await client.PostAsync("/api/webhooks/incoming/salesforce", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedFaultString, responseText);
        }

        [Fact]
        public async Task WebHookAction_WithWrongOrganizationId_IsBadRequest()
        {
            // Arrange
            var expectedFaultString = "<faultstring>The '/*[local-name()='Body']/*[local-name()='notifications']/*" +
                "[local-name()='OrganizationId']' value provided in the HTTP request body did not match the " +
                "expected value.</faultstring>";

            var fixture = _fixture.WithTestLogger(out var testSink);
            var client = fixture.CreateClient();

            var path = Path.Combine("Resources", "RequestBodies", "Salesforce.BadSecret.xml");
            var stream = await ResourceFile.GetResourceStreamAsync(path, normalizeLineEndings: true);
            var content = new StreamContent(stream)
            {
                Headers =
                {
                    { HeaderNames.ContentLength, stream.Length.ToString() },
                    { HeaderNames.ContentType, "text/xml" },
                },
            };

            // Act
            var response = await client.PostAsync("/api/webhooks/incoming/salesforce", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedFaultString, responseText);
        }
    }
}
