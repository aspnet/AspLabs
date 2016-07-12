// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Utilities
{
    public class EncodingUtilitiesTests
    {
        public static TheoryData<string> HexData
        {
            get
            {
                return new TheoryData<string>
                {
                    string.Empty,
                    " ",
                    "\r\n",
                    "text",
                    "你好世界",
                    new string('你', 16 * 1024)
                };
            }
        }

        public static TheoryData<string> InvalidHexData
        {
            get
            {
                return new TheoryData<string>
                {
                    "E4BDA0E5A5BDE4B896E7958",
                    "4BDA0E5A5BDE4B896E7958C",
                    "E4BDA0E5A5MDE4B896E7958C"
                };
            }
        }

        [Theory]
        [MemberData(nameof(HexData))]
        public void ToHex_ConvertsCorrectly(string input)
        {
            // Arrange
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string expected = ToExpectedHex(bytes);

            // Act
            string actual = EncodingUtilities.ToHex(bytes);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(HexData))]
        public void FromHex_ConvertsCorrectly(string input)
        {
            // Arrange
            byte[] expected = Encoding.UTF8.GetBytes(input);
            string data1 = ToExpectedHex(expected).ToUpperInvariant();
            string data2 = ToExpectedHex(expected).ToLowerInvariant();

            // Act
            byte[] actual1 = EncodingUtilities.FromHex(data1);
            byte[] actual2 = EncodingUtilities.FromHex(data2);

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(expected, actual2);
        }

        [Theory]
        [MemberData(nameof(InvalidHexData))]
        public void FromHex_Throws_OnOddInput(string invalid)
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => EncodingUtilities.FromHex(invalid));

            Assert.Contains(string.Format(CultureInfo.CurrentCulture, "Input is not a valid hex-encoded string: '{0}'. Please provide a valid hex-encoded string.", invalid), ex.Message);
        }

        [Theory]
        [MemberData(nameof(HexData))]
        public void ToHex_FromHex_Roundtrips_UriSafe(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            string encoded = EncodingUtilities.ToHex(data);
            byte[] output = EncodingUtilities.FromHex(encoded);
            string actual = Encoding.UTF8.GetString(output);
            Assert.Equal(input, actual);
        }

        [Theory]
        [MemberData(nameof(HexData))]
        public void ToBase64_FromBase64_Roundtrips_UriSafe(string input)
        {
            Base64RoundTrip(input, uriSafe: true);
        }

        [Theory]
        [MemberData(nameof(HexData))]
        public void ToBase64_FromBase64_Roundtrips_NotUriSafe(string input)
        {
            Base64RoundTrip(input, uriSafe: false);
        }

        private static string ToExpectedHex(byte[] bytes)
        {
            if (bytes == null)
            {
                return string.Empty;
            }
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        private static void Base64RoundTrip(string input, bool uriSafe)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            string encoded = EncodingUtilities.ToBase64(data, uriSafe);
            byte[] output = EncodingUtilities.FromBase64(encoded);
            string actual = Encoding.UTF8.GetString(output);
            Assert.Equal(input, actual);
        }
    }
}