// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace System
{
    public class UriExtensionsTests
    {
        public static TheoryData<string, bool> HttpData
        {
            get
            {
                return new TheoryData<string, bool>
                {
                    { "relative", false },
                    { "/some/path", false },
                    { "https://localhost", false },
                    { "ftp://localhost", false },
                    { "ftps://localhost", false },
                    { "telnet://localhost", false },
                    { "http://localhost", true },
                    { "HTTP://localhost", true },
                    { "Http://localhost", true },
                };
            }
        }

        public static TheoryData<string, bool> HttpsData
        {
            get
            {
                return new TheoryData<string, bool>
                {
                    { "relative", false },
                    { "/some/path", false },
                    { "http://localhost", false },
                    { "ftp://localhost", false },
                    { "ftps://localhost", false },
                    { "telnet://localhost", false },
                    { "https://localhost", true },
                    { "HTTPS://localhost", true },
                    { "HttpS://localhost", true },
                };
            }
        }

        [Fact]
        public void IsHttp_HandlesNull()
        {
            // Arrange
            Uri address = null;

            // Act
            bool actual = address.IsHttp();

            // Assert
            Assert.False(actual);
        }

        [Theory]
        [MemberData("HttpData")]
        public void IsHttp_DetectsHttpUris(string input, bool expected)
        {
            // Arrange
            Uri address = new Uri(input, UriKind.RelativeOrAbsolute);

            // Act
            bool actual = address.IsHttp();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsHttps_HandlesNull()
        {
            // Arrange
            Uri address = null;

            // Act
            bool actual = address.IsHttps();

            // Assert
            Assert.False(actual);
        }

        [Theory]
        [MemberData("HttpsData")]
        public void IsHttps_DetectsHttpsUris(string input, bool expected)
        {
            // Arrange
            Uri address = new Uri(input, UriKind.RelativeOrAbsolute);

            // Act
            bool actual = address.IsHttps();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
