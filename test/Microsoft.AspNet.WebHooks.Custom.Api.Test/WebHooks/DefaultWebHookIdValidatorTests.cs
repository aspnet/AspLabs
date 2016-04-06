// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.WebHooks.WebHooks
{
    public class DefaultWebHookIdValidatorTests
    {
        private readonly HttpRequestMessage _request;
        private readonly IWebHookIdValidator _validator;

        public DefaultWebHookIdValidatorTests()
        {
            _request = new HttpRequestMessage();
            _validator = new DefaultWebHookIdValidator();
        }

        [Theory]
        [InlineData("a")]
        [InlineData("12345")]
        [InlineData("你好世界")]
        public async Task ValidateIfAsync_ForcesDefaultId(string id)
        {
            // Arrange
            WebHook webHook = new WebHook { Id = id };

            // Act
            await _validator.ValidateIdAsync(_request, webHook);

            // Assert
            Assert.NotEmpty(webHook.Id);
            Assert.NotEqual(id, webHook.Id);
        }
    }
}
