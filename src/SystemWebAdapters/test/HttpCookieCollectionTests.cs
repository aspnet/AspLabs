// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "Need to verify collection implementation")]
public class HttpCookieCollectionTests
{
    private readonly Fixture _fixture;

    public HttpCookieCollectionTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void RequestCookiesNotSupportedMethods()
    {
        // Arrange
        var requestCookies = new Mock<IRequestCookieCollection>();
        var cookies = new HttpCookieCollection(requestCookies.Object);

        // Act/Assert
        Assert.Throws<PlatformNotSupportedException>(() => cookies.Clear());
        Assert.Throws<PlatformNotSupportedException>(() => cookies.Add(new HttpCookie(_fixture.Create<string>())));
        Assert.Throws<PlatformNotSupportedException>(() => cookies.Set(new HttpCookie(_fixture.Create<string>())));
        Assert.Throws<PlatformNotSupportedException>(() => cookies.Remove(_fixture.Create<string>()));
    }

    [Fact]
    public void RequestCookiesSingleItem()
    {
        // Arrange
        var count = 1;
        var requestCookies = new Mock<IRequestCookieCollection>();
        requestCookies.Setup(c => c.Count).Returns(count);

        var cookies = new HttpCookieCollection(requestCookies.Object);

        // Act
        var result = cookies.Count;

        // Assert
        Assert.Equal(result, cookies.Count);
    }

    [Fact]
    public void RequestGetCookieDoesNotExist()
    {
        // Arrange
        var requestCookies = new Mock<IRequestCookieCollection>();
        var cookies = new HttpCookieCollection(requestCookies.Object);

        // Act
        var result = cookies[_fixture.Create<string>()];

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RequestGetCookie()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();
        var requestCookies = new Mock<IRequestCookieCollection>();
        requestCookies.Setup(r => r.TryGetValue(key, out value)).Returns(true);

        var cookies = new HttpCookieCollection(requestCookies.Object);

        // Act
        var result = cookies[key]!;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result.Value);
        Assert.Equal(key, result.Name);
    }

    [Fact]
    public void ResponseCookiesNotSupportedMethods()
    {
        // Arrange
        var responseCookies = new Mock<IResponseCookies>();
        var cookies = new HttpCookieCollection(responseCookies.Object);

        // Act/Assert
        Assert.Throws<PlatformNotSupportedException>(() => cookies.Clear());
        Assert.Throws<PlatformNotSupportedException>(() => cookies.AllKeys);
        Assert.Throws<PlatformNotSupportedException>(() => cookies.Count);
        Assert.Throws<PlatformNotSupportedException>(() => cookies[_fixture.Create<string>()]);
    }

    [Fact]
    public void ResponseSet()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var value = _fixture.Create<string>();
        var responseCookies = new Mock<IResponseCookies>();
        var cookies = new HttpCookieCollection(responseCookies.Object);

        var cookie = new HttpCookie(name, value)
        {
            Domain = _fixture.Create<string>(),
            Expires = DateTime.UtcNow,
            HttpOnly = _fixture.Create<bool>(),
            Path = _fixture.Create<string>(),
            SameSite = _fixture.Create<SameSiteMode>(),
            Secure = _fixture.Create<bool>(),
        };

        // Act
        cookies.Set(cookie);

        // Assert
        responseCookies.Verify(v => v.Delete(name), Times.Once);
        responseCookies.Verify(v => v.Append(name, value, It.Is<CookieOptions>(t =>
            t.Secure == cookie.Secure && t.HttpOnly == cookie.HttpOnly && t.Domain == cookie.Domain && t.Expires == cookie.Expires && t.Path == cookie.Path && t.SameSite == (Microsoft.AspNetCore.Http.SameSiteMode)cookie.SameSite)));
    }

    [Fact]
    public void ResponseAdd()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var value = _fixture.Create<string>();
        var responseCookies = new Mock<IResponseCookies>();
        var cookies = new HttpCookieCollection(responseCookies.Object);

        var cookie = new HttpCookie(name, value)
        {
            Domain = _fixture.Create<string>(),
            Expires = DateTime.UtcNow,
            HttpOnly = _fixture.Create<bool>(),
            Path = _fixture.Create<string>(),
            SameSite = _fixture.Create<SameSiteMode>(),
            Secure = _fixture.Create<bool>(),
        };

        // Act
        cookies.Add(cookie);

        // Assert
        responseCookies.Verify(v => v.Delete(name), Times.Never);
        responseCookies.Verify(v => v.Append(name, value, It.Is<CookieOptions>(t =>
            t.Secure == cookie.Secure && t.HttpOnly == cookie.HttpOnly && t.Domain == cookie.Domain && t.Expires == cookie.Expires && t.Path == cookie.Path && t.SameSite == (Microsoft.AspNetCore.Http.SameSiteMode)cookie.SameSite)));
    }

    [Fact]
    public void ResponseRemove()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var responseCookies = new Mock<IResponseCookies>();
        var cookies = new HttpCookieCollection(responseCookies.Object);

        // Act
        cookies.Remove(name);

        // Assert
        responseCookies.Setup(r => r.Delete(name));
    }
}
