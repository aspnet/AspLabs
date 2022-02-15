// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace System.Web
{
    public class HttpRequestTests
    {
        private readonly Fixture _fixture;

        public HttpRequestTests()
        {
            _fixture = new Fixture();
            _fixture.Register(() => new IPAddress(_fixture.Create<long>()));
        }

        [Fact]
        public void Path()
        {
            // Arrange
            var path = new PathString("/" + _fixture.Create<string>());
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Path).Returns(path);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.Path;

            // Assert
            Assert.Equal(path.Value, result);
        }

        [Fact]
        public void Url()
        {
            // Arrange
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Scheme).Returns("http");
            coreRequest.Setup(c => c.Host).Returns(new HostString("microsoft.com"));
            coreRequest.Setup(c => c.PathBase).Returns("/path/base");
            coreRequest.Setup(c => c.QueryString).Returns(new QueryString("?key=value&key2=value%20with%20space"));

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.Url;

            // Assert
            Assert.Equal(new Uri("http://microsoft.com/path/base?key=value&key2=value with space"), result);
        }

        [Fact]
        public void RawUrl()
        {
            // Arrange
            var rawurl = _fixture.Create<string>();
            var features = new FeatureCollection();
            features.Set(new SystemWebAdapterMetadata { RawUrl = rawurl });

            var context = new Mock<HttpContextCore>();
            context.Setup(c => c.Features).Returns(features);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(context.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.RawUrl;

            // Assert
            Assert.Equal(rawurl, result);
        }

        [Fact]
        public void RawUrlNotAvailable()
        {
            // Arrange
            var features = new FeatureCollection();
            var context = new Mock<HttpContextCore>();
            context.Setup(c => c.Features).Returns(features);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(context.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            Assert.Throws<InvalidOperationException>(() => request.RawUrl);
        }

        [Fact]
        public void HttpMethod()
        {
            // Arrange
            var method = _fixture.Create<string>();
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Method).Returns(method);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.HttpMethod;

            // Assert
            Assert.Equal(method, result);
        }

        [Fact]
        public void UserHostAddress()
        {
            // Arrange
            var host = new HostString(_fixture.Create<string>());
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Host).Returns(host);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UserHostAddress;

            // Assert
            Assert.Equal(host.Value, result);
        }

        [Fact]
        public void UserLanguagesEmpty()
        {
            // Arrange
            var headers = new Mock<IHeaderDictionary>();
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Headers).Returns(headers.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UserLanguages;

            // Assert
            Assert.Empty(result);
            Assert.Same(Array.Empty<string>(), result);
        }

        [Fact]
        public void UserLanguagesTwoItems()
        {
            // Arrange
            var headers = new HeaderDictionary
            {
                { HeaderNames.AcceptLanguage, "en;q=0.9, ru;q=0.5, de;q=0.7"}
            };

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Headers).Returns(headers);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UserLanguages;

            // Assert
            Assert.Equal(new[] { "en", "de", "ru" }, result);
            Assert.Same(result, request.UserLanguages);
        }

        [Fact]
        public void UserLanguagesTwoItemsNoQuality()
        {
            // Arrange
            var headers = new HeaderDictionary
            {
                { HeaderNames.AcceptLanguage, "en;q=0.9, ru, de;q=0.7"}
            };

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Headers).Returns(headers);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UserLanguages;

            // Assert
            Assert.Equal(new[] { "ru", "en", "de" }, result);
            Assert.Same(result, request.UserLanguages);
        }

        [Fact]
        public void UserAgent()
        {
            // Arrange
            var userAgent = _fixture.Create<string>();
            var headers = new HeaderDictionary
            {
                { HeaderNames.UserAgent, userAgent }
            };

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Headers).Returns(headers);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UserAgent;

            // Assert
            Assert.Equal(userAgent, result);
        }

        [Fact]
        public void RequestType()
        {
            // Arrange
            var method = _fixture.Create<string>();
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Method).Returns(method);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.RequestType;

            // Assert
            Assert.Equal(method, result);
        }


        [Fact]
        public void ContentLengthEmpty()
        {
            // Arrange
            var coreRequest = new Mock<HttpRequestCore>();
            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.ContentLength;

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void ContentLength()
        {
            // Arrange
            var length = _fixture.Create<int>();
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.ContentLength).Returns(length);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.ContentLength;

            // Assert
            Assert.Equal(length, result);
        }

        [Fact]
        public void ContentType()
        {
            // Arrange
            var contentType = _fixture.Create<string>();
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.ContentType).Returns(contentType);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.ContentType;

            // Assert
            Assert.Equal(contentType, result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IsSecureConnection(bool isHttps)
        {
            // Arrange
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.IsHttps).Returns(isHttps);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.IsSecureConnection;

            // Assert
            Assert.Equal(isHttps, result);
        }

        [Fact]
        public void IsLocalNoAddresses()
        {
            // Arrange
            var info = new Mock<ConnectionInfo>();

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Connection).Returns(info.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.IsLocal;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsLocalRemoteIsLocalAddress()
        {
            // Arrange
            var ipAddress = _fixture.Create<IPAddress>();
            var info = new Mock<ConnectionInfo>();
            info.Setup(i => i.RemoteIpAddress).Returns(ipAddress);
            info.Setup(i => i.LocalIpAddress).Returns(ipAddress);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Connection).Returns(info.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.IsLocal;

            // Assert
            Assert.True(result);
        }

        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        [Theory]
        public void IsLocal(bool isRemoteLoopback, bool isLocalLoopback, bool isLocal)
        {
            // Arrange
            var remote = isRemoteLoopback ? IPAddress.Loopback : _fixture.Create<IPAddress>();
            var local = isLocalLoopback ? IPAddress.Loopback : _fixture.Create<IPAddress>();

            var info = new Mock<ConnectionInfo>();
            info.Setup(i => i.RemoteIpAddress).Returns(remote);
            info.Setup(i => i.LocalIpAddress).Returns(local);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Connection).Returns(info.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.IsLocal;

            // Assert
            Assert.Equal(isLocal, result);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void IsLocalNullLocal(bool isLoopback)
        {
            // Arrange
            var remote = isLoopback ? IPAddress.Loopback : _fixture.Create<IPAddress>();

            var info = new Mock<ConnectionInfo>();
            info.Setup(i => i.RemoteIpAddress).Returns(remote);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Connection).Returns(info.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.IsLocal;

            // Assert
            Assert.Equal(isLoopback, result);
        }

        [Fact]
        public void AppRelativeCurrentExecutionFilePath()
        {
            // Arrange
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Scheme).Returns("http");
            coreRequest.Setup(c => c.Host).Returns(new HostString("www.A.com"));
            coreRequest.Setup(c => c.Path).Returns("/B/C");
            coreRequest.Setup(c => c.QueryString).Returns(new QueryString("?D=E"));
            coreRequest.Setup(c => c.PathBase).Returns("/F");

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var url = request.Url;
            var result = request.AppRelativeCurrentExecutionFilePath;

            // Assert
            Assert.Equal("~/B/C", result);
        }

        [Fact]
        public void ApplicationPath()
        {
            // Arrange
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Scheme).Returns("http");
            coreRequest.Setup(c => c.Host).Returns(new HostString("www.A.com"));
            coreRequest.Setup(c => c.Path).Returns("/B/C");
            coreRequest.Setup(c => c.QueryString).Returns(new QueryString("?D=E"));
            coreRequest.Setup(c => c.PathBase).Returns("/F");

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var url = request.Url;
            var result = request.ApplicationPath;

            // Assert
            Assert.Equal("/F", result);
        }

        [Fact]
        public void UrlReferrer()
        {
            // Arrange
            var referrer = "http://contoso.com";
            var headers = new HeaderDictionary
            {
                { HeaderNames.Referer, referrer },
            };

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Headers).Returns(headers);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UrlReferrer;

            // Assert
            Assert.Equal(new Uri(referrer), result);
        }

        [Fact]
        public void TotalBytes()
        {
            // Arrange
            var length = _fixture.Create<int>();
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.ContentLength).Returns(length);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.TotalBytes;

            // Assert
            Assert.Equal(length, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsAuthenticated(bool isAuthenticated)
        {
            // Arrange
            var identity = new Mock<IIdentity>();
            identity.Setup(i => i.IsAuthenticated).Returns(isAuthenticated);

            var user = new Mock<ClaimsPrincipal>();
            user.Setup(u => u.Identity).Returns(identity.Object);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.User).Returns(user.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.IsAuthenticated;

            // Assert
            Assert.Equal(isAuthenticated, result);
        }

        [Fact]
        public void Identity()
        {
            // Arrange
            var identity = new Mock<IIdentity>();

            var user = new Mock<ClaimsPrincipal>();
            user.Setup(u => u.Identity).Returns(identity.Object);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.User).Returns(user.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.LogonUserIdentity;

            // Assert
            Assert.Same(identity.Object, result);
        }

        public enum ContentEncodingType
        {
            None,
            UTF8,
            UTF32,
        }

        [Theory]
        [InlineData(null, ContentEncodingType.None)]
        [InlineData("application/json;charset=utf-8", ContentEncodingType.UTF8)]
        [InlineData("application/json;charset=utf-32", ContentEncodingType.UTF32)]
        public void ContentEncoding(string contentType, ContentEncodingType type)
        {
            // Arrange
            var headers = new HeaderDictionary
            {
                { HeaderNames.ContentType, contentType  }
            };

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.Headers).Returns(headers);

            var request = new HttpRequest(coreRequest.Object);
            var expected = type switch
            {
                ContentEncodingType.None => null,
                ContentEncodingType.UTF8 => Encoding.UTF8,
                ContentEncodingType.UTF32 => Encoding.UTF32,
                _ => throw new NotImplementedException(),
            };

            // Act
            var result = request.ContentEncoding;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void UserHostName()
        {
            // Arrange
            var remote = _fixture.Create<IPAddress>();
            var info = new Mock<ConnectionInfo>();
            info.Setup(i => i.RemoteIpAddress).Returns(remote);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Connection).Returns(info.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UserHostName;

            // Assert
            Assert.Equal(remote.ToString(), result);
        }

        [Fact]
        public void UserHostNameNoRemote()
        {
            // Arrange
            var info = new Mock<ConnectionInfo>();

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Connection).Returns(info.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UserHostName;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Abort()
        {
            // Arrange
            var coreContext = new Mock<HttpContextCore>();
            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            request.Abort();

            // Assert
            coreContext.Verify(c => c.Abort(), Times.Once());
        }
    }
}
