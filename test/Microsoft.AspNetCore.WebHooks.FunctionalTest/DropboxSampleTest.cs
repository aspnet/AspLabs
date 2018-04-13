// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DropboxCoreReceiver;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.FunctionalTest
{
    public class DropboxSampleTest : IClassFixture<WebHookTestFixture<Startup>>
    {
        private readonly HttpClient _client;

        public DropboxSampleTest(WebHookTestFixture<Startup> fixture)
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
        public async Task WebHookPage_Get_ReturnsChallenge()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/webhooks/incoming/dropbox?challenge=012345678901234");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal("012345678901234", responseText);
        }
    }
}
