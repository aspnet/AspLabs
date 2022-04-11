// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Xunit;

namespace System.Web;

public class HttpServerUtilityTests
{
    private readonly Fixture _fixture;

    public HttpServerUtilityTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void UrlTokenEncodeEmpty()
    {
        // Act
        var encoded = HttpServerUtility.UrlTokenDecode(string.Empty);

        // Assert
        Assert.Same(Array.Empty<byte>(), encoded);
    }

    [Fact]
    public void UrlTokenNull()
    {
        Assert.Throws<ArgumentNullException>(() => HttpServerUtility.UrlTokenDecode(null!));
        Assert.Throws<ArgumentNullException>(() => HttpServerUtility.UrlTokenEncode(null!));
    }

    [InlineData(11)]
    [InlineData(-1)]
    [Theory]
    public void UrlTokenDecodeInvalidPaddingCount(int finalValue)
    {
        // Arrange
        var padChar = (char)('0' + finalValue);
        var input = "aa" + padChar;

        // Act
        var result = HttpServerUtility.UrlTokenDecode(input);

        // Assert
        Assert.Null(result);
    }

    [InlineData(new byte[] { 185, 178, 254 }, "ubL-0")] // base64 contains +
    [InlineData(new byte[] { 253, 7, 171 }, "_Qer0")] // base64 contains /
    [InlineData(new byte[] { 211, 90, 167, 128, 197 }, "01qngMU1")] // base64 contains padding
    [Theory]
    public void UrlTokenRoundtripBytes(byte[] bytes, string expected)
    {
        Assert.Equal(expected, HttpServerUtility.UrlTokenEncode(bytes));
        Assert.Equal(bytes, HttpServerUtility.UrlTokenDecode(expected));
    }

    [InlineData("", "")]
    [InlineData("a", "YQ2")]
    [InlineData("j~", "an41")]
    [Theory]
    public void UrlTokenRoundtrip(string input, string expected)
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var encoded = HttpServerUtility.UrlTokenEncode(bytes);
        var decoded = HttpServerUtility.UrlTokenDecode(expected);

        // Assert
        Assert.Equal(expected, encoded);
        Assert.Equal(bytes, decoded);
    }
}
