// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using AutoFixture;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpCookieTests
{
    private readonly Fixture _fixture;

    public HttpCookieTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void DefaultValues()
    {
        // Act
        var cookie = new HttpCookie(_fixture.Create<string>());

        // Assert
        Assert.Null(cookie.Domain);
        Assert.Equal("/", cookie.Path);
        Assert.Equal(SameSiteMode.Lax, cookie.SameSite);
        Assert.False(cookie.Secure);
    }

    [Fact]
    public void Create()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        // Act
        var cookie = new HttpCookie(name, value);

        // Assert
        Assert.Single(cookie.Values);
        Assert.Equal(value, cookie.Values[null]);
    }

    [Fact]
    public void CreateMultivalues()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var key1 = _fixture.Create<string>();
        var value1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();
        var value2 = _fixture.Create<string>();

        var full = $"{key1}={value1}&{key2}={value2}";

        // Act
        var cookie = new HttpCookie(name, full);

        // Assert
        Assert.Equal(full, cookie.Value);
        Assert.Equal(value1, cookie.Values[key1]);
        Assert.Equal(value2, cookie.Values[key2]);
    }

    [Fact]
    public void Add()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var key1 = _fixture.Create<string>();
        var value1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();
        var value2 = _fixture.Create<string>();

        var full = $"{key1}={value1}&{key2}={value2}";

        // Act
        var cookie = new HttpCookie(name);
        cookie.Values.Add(key1, value1);
        cookie.Values.Add(key2, value2);

        // Assert
        Assert.Equal(full, cookie.Value);
        Assert.Equal(value1, cookie.Values[key1]);
        Assert.Equal(value2, cookie.Values[key2]);
    }

    [Fact]
    public void SetValues()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var key1 = _fixture.Create<string>();
        var value1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();
        var value2 = _fixture.Create<string>();

        var full = $"{key1}={value1}&{key2}={value2}";

        // Act
        var cookie = new HttpCookie(name)
        {
            Value = full
        };

        // Assert
        Assert.Equal(full, cookie.Value);
        Assert.Equal(value1, cookie.Values[key1]);
        Assert.Equal(value2, cookie.Values[key2]);
    }

    [Fact]
    public void OverrideValues()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        // Act
        var cookie = new HttpCookie(name);
        cookie.Values.Add(_fixture.Create<string>(), _fixture.Create<string>());
        cookie.Value = value;

        // Assert
        Assert.Equal(value, cookie.Value);
        Assert.Single(cookie.Values);
        Assert.Equal(value, cookie.Values[null]);
    }

    [Fact]
    public void NoExpireSet()
    {
        // Act
        var cookie = new HttpCookie(_fixture.Create<string>());

        // Assert
        Assert.Equal(DateTime.MinValue, cookie.Expires);
    }

    [Fact]
    public void SetExpires()
    {
        // Arrange
        var expires = DateTime.UtcNow;

        // Act
        var cookie = new HttpCookie(_fixture.Create<string>())
        {
            Expires = expires
        };

        // Assert
        Assert.Equal(expires, cookie.Expires);
    }
}
