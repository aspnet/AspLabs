// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace System
{
    public class UriExtensionsTests
    {
        public static TheoryData<string, bool> HttpsData
        {
            get
            {
                return new TheoryData<string, bool>
                {
                    { "http://localhost", false },
                    { "https://localhost", true },
                    { "HTTPS://localhost", true },
                    { "HttpS://localhost", true },
                };
            }
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
            Uri address = new Uri(input);

            // Act
            bool actual = address.IsHttps();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
