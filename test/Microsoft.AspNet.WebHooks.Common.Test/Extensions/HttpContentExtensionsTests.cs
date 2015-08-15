// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Extensions
{
    public class HttpContentExtensionsTests
    {
        public static TheoryData<string, bool> JsonMediaTypes
        {
            get
            {
                return new TheoryData<string, bool>
                {
                    { "app/json", false },
                    { "other/json", false },
                    { "applicationnnn/json", false },
                    { "texttttt/json", false },
                    { "aaaplication/json", false },
                    { "tttext/json", false },
                    { "aplication/jsonxzy", false },
                    { "text/jsonxyz", false },
                    { "application/json", true },
                    { "application/xyzjson", false },
                    { "text/xyzjson", false },
                    { "application/hal-json", false },
                    { "application/hal;json", false },
                    { "application/hal+json", true },
                    { "text/json", true },
                    { "text/hal-json", false },
                    { "text/hal;json", false },
                    { "text/hal+json", true },
                };
            }
        }

        [Fact]
        public void IsJson_HandlesNullContent()
        {
            // Arrange
            HttpContent content = null;

            // Act
            bool actual = content.IsJson();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsJson_HandlesNullContentType()
        {
            // Arrange
            HttpContent content = new StringContent(string.Empty);
            content.Headers.ContentType = null;

            // Act
            bool actual = content.IsJson();

            // Assert
            Assert.False(actual);
        }

        [Theory]
        [MemberData("JsonMediaTypes")]
        public void IsJson_DetectsJson(string input, bool expected)
        {
            // Arrange
            HttpContent content = new StringContent(string.Empty);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(input);

            // Act
            bool actual = content.IsJson();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
