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
                    { "application/hal+json;x=y", true },
                    { "text/json", true },
                    { "text/json;x=y", true },
                    { "text/hal-json", false },
                    { "text/hal;json", false },
                    { "text/hal+json", true },
                    { "text/hal+json;x=y", true },
                };
            }
        }

        public static TheoryData<string, bool> XmlMediaTypes
        {
            get
            {
                return new TheoryData<string, bool>
                {
                    { "app/xml", false },
                    { "other/xml", false },
                    { "applicationnnn/xml", false },
                    { "texttttt/xml", false },
                    { "aaaplication/xml", false },
                    { "tttext/xml", false },
                    { "aplication/xmlxzy", false },
                    { "text/xmlxyz", false },
                    { "application/xml", true },
                    { "application/xyzxml", false },
                    { "text/xyzxml", false },
                    { "application/hal-xml", false },
                    { "application/hal;xml", false },
                    { "application/hal+xml", true },
                    { "application/hal+xml;x=y", true },
                    { "text/xml", true },
                    { "text/xml;x=y", true },
                    { "text/hal-xml", false },
                    { "text/hal;xml", false },
                    { "text/hal+xml", true },
                    { "text/hal+xml;x=y", true },
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

        [Fact]
        public void IsXml_HandlesNullContent()
        {
            // Arrange
            HttpContent content = null;

            // Act
            bool actual = content.IsXml();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsXml_HandlesNullContentType()
        {
            // Arrange
            HttpContent content = new StringContent(string.Empty);
            content.Headers.ContentType = null;

            // Act
            bool actual = content.IsXml();

            // Assert
            Assert.False(actual);
        }

        [Theory]
        [MemberData("XmlMediaTypes")]
        public void IsXml_DetectsXml(string input, bool expected)
        {
            // Arrange
            HttpContent content = new StringContent(string.Empty);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(input);

            // Act
            bool actual = content.IsXml();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
