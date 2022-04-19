// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Text;
using System.Threading;
using AutoFixture;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpResponseTests
{
    private readonly Fixture _fixture;

    public HttpResponseTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void StatusCode()
    {
        // Arrange
        var responseCore = new Mock<HttpResponseCore>();
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.StatusCode = 205;

        // Assert
        Assert.Equal(205, response.StatusCode);
        Assert.Equal(205, responseCore.Object.StatusCode);
    }

    [Fact]
    public void StatusDescription()
    {
        // Arrange
        var description = _fixture.Create<string>();
        var feature = new Mock<IHttpResponseFeature>();
        feature.SetupProperty(f => f.ReasonPhrase);

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IHttpResponseFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.StatusDescription = description;

        // Assert
        Assert.Equal(description, feature.Object.ReasonPhrase);
        Assert.Equal(description, response.StatusDescription);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TrySkipIisCustomErrors(bool isEnabled)
    {
        // Arrange
        var description = _fixture.Create<string>();
        var feature = new Mock<IStatusCodePagesFeature>();
        feature.SetupProperty(f => f.Enabled);

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IStatusCodePagesFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.TrySkipIisCustomErrors = isEnabled;

        // Assert
        Assert.Equal(isEnabled, feature.Object.Enabled);
        Assert.Equal(isEnabled, response.TrySkipIisCustomErrors);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SuppressContent(bool suppressContent)
    {
        // Arrange
        var feature = new Mock<IBufferedResponseFeature>();
        feature.SetupProperty(f => f.SuppressContent);

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IBufferedResponseFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.SuppressContent = suppressContent;

        // Assert
        Assert.Equal(suppressContent, feature.Object.SuppressContent);
        Assert.Equal(suppressContent, response.SuppressContent);
    }

    [Fact]
    public void End()
    {
        // Arrange
        var feature = new Mock<IBufferedResponseFeature>();

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IBufferedResponseFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.End();

        // Assert
        feature.Verify(f => f.End(), Times.Once);
    }

    [Fact]
    public void EndNoFeature()
    {
        // Arrange
        var features = new Mock<IFeatureCollection>();

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);

        var response = new HttpResponse(responseCore.Object);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => response.End());
    }

    [Fact]
    public void ContentType()
    {
        // Arrange
        var headers = new HeaderDictionary();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Headers).Returns(headers);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ContentType = "application/json";

        // Assert
        Assert.Equal("application/json", headers[HeaderNames.ContentType]);
    }

    [Fact]
    public void ContentTypeWithEncoding()
    {
        // Arrange
        var headers = new HeaderDictionary();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Headers).Returns(headers);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ContentType = "application/json";
        response.ContentEncoding = Encoding.UTF32;

        // Assert
        Assert.Equal("application/json; charset=utf-32", headers[HeaderNames.ContentType]);
    }

    [Fact]
    public void ContentTypeWithCharset()
    {
        // Arrange
        var headers = new HeaderDictionary();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Headers).Returns(headers);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ContentType = "application/json";
        response.Charset = Encoding.UTF32.WebName;

        // Assert
        Assert.Equal("application/json; charset=utf-32", headers[HeaderNames.ContentType]);
    }

    [Fact]
    public void Headers()
    {
        // Arrange
        var headersCore = new HeaderDictionary();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Headers).Returns(headersCore);

        var response = new HttpResponse(responseCore.Object);

        // Act
        var headers1 = response.Headers;
        var headers2 = response.Headers;

        // Assert
        Assert.Same(headers1, headers2);
        Assert.IsType<StringValuesDictionaryNameValueCollection>(headers1);
    }

    [Fact]
    public void Clear()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { HeaderNames.ContentType, "application/json" },
        };

        var feature = new Mock<IBufferedResponseFeature>();
        var responseFeature = new Mock<IHttpResponseFeature>();

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IBufferedResponseFeature>()).Returns(feature.Object);
        features.Setup(f => f.Get<IHttpResponseFeature>()).Returns(responseFeature.Object);

        var body = new Mock<Stream>();

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.SetupProperty(r => r.StatusCode);
        responseCore.Setup(r => r.Body).Returns(body.Object);
        responseCore.Setup(r => r.Headers).Returns(headers);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.Clear();

        // Assert
        feature.Verify(f => f.ClearContent(), Times.Once);
        Assert.Empty(headers);
    }

    [Fact]
    public void ClearContentsStreamNotSeekable()
    {
        // Arrange
        var feature = new Mock<IBufferedResponseFeature>();
        var body = new Mock<Stream>();

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IBufferedResponseFeature>()).Returns(feature.Object);

        var context = new Mock<HttpContextCore>();
        context.Setup(c => c.Features).Returns(features.Object);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);
        responseCore.Setup(r => r.Body).Returns(body.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ClearContent();

        // Assert
        feature.Verify(f => f.ClearContent(), Times.Once);
    }

    [Fact]
    public void ClearContentsStreamSeekable()
    {
        // Arrange
        var body = new Mock<Stream>();
        body.Setup(b => b.CanSeek).Returns(true);

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Body).Returns(body.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        response.ClearContent();

        // Assert
        body.Verify(b => b.SetLength(0), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsClientConnected(bool isConnected)
    {
        // Arrange
        var context = new Mock<HttpContextCore>();
        context.SetupProperty(c => c.RequestAborted);
        using var cts = new CancellationTokenSource();

        if (!isConnected)
        {
            cts.Cancel();
        }

        context.Object.RequestAborted = cts.Token;

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.HttpContext).Returns(context.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        var result = response.IsClientConnected;

        // Assert
        Assert.Equal(isConnected, result);
    }

    [Fact]
    public void Cookies()
    {
        // Arrange
        var cookies = new Mock<IResponseCookies>();

        var responseCore = new Mock<HttpResponseCore>();
        responseCore.Setup(r => r.Cookies).Returns(cookies.Object);

        var response = new HttpResponse(responseCore.Object);

        // Act
        var cookies1 = response.Cookies;
        var cookies2 = response.Cookies;

        // Assert
        Assert.Same(cookies1, cookies2);
    }
}
