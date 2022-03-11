// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web.Internal;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
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
            _fixture.Register(() =>
            {
                var address = IPAddress.Loopback;

                while (IPAddress.IsLoopback(address))
                {
                    address = new IPAddress(_fixture.Create<long>());
                }

                return address;
            });
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

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void UserHostAddress(bool hasRemote)
        {
            // Arrange
            var info = new Mock<ConnectionInfo>();
            var remoteIp = hasRemote ? _fixture.Create<IPAddress>() : null;
            info.Setup(i => i.RemoteIpAddress).Returns(remoteIp!);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Connection).Returns(info.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(r => r.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.UserHostAddress;

            // Assert
            Assert.Equal(remoteIp?.ToString(), result);
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

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void IsLocalRemoteNull(bool isLoopback)
        {
            // Arrange
            var local = isLoopback ? IPAddress.Loopback : _fixture.Create<IPAddress>();
            var info = new Mock<ConnectionInfo>();
            info.Setup(i => i.LocalIpAddress).Returns(local);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Connection).Returns(info.Object);

            var coreRequest = new Mock<HttpRequestCore>();
            coreRequest.Setup(c => c.HttpContext).Returns(coreContext.Object);

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var result = request.IsLocal;

            // Assert
            Assert.False(result);
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
        [InlineData(true, false, true)]
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
            coreRequest.Setup(c => c.Path).Returns("/B/ C");
            coreRequest.Setup(c => c.QueryString).Returns(new QueryString("?D=E"));
            coreRequest.Setup(c => c.PathBase).Returns("/F");

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var url = request.Url;
            var result = request.AppRelativeCurrentExecutionFilePath;

            // Assert
            Assert.Equal("~/B/ C", result);
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
            coreRequest.Setup(c => c.PathBase).Returns("/F/ C");

            var request = new HttpRequest(coreRequest.Object);

            // Act
            var url = request.Url;
            var result = request.ApplicationPath;

            // Assert
            Assert.Equal("/F/ C", result);
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

        [Fact]
        public void Headers()
        {
            // Arrange
            var headersCore = new HeaderDictionary();

            var requestCore = new Mock<HttpRequestCore>();
            requestCore.Setup(r => r.Headers).Returns(headersCore);

            var request = new HttpRequest(requestCore.Object);

            // Act
            var headers1 = request.Headers;
            var headers2 = request.Headers;

            // Assert
            Assert.Same(headers1, headers2);
            Assert.IsType<StringValuesDictionaryNameValueCollection>(headers1);
        }

        [Fact]
        public void ServerVariables()
        {
            // Arrange
            var serverVariables = new Mock<IServerVariablesFeature>();

            var features = new Mock<IFeatureCollection>();
            features.Setup(f => f.Get<IServerVariablesFeature>()).Returns(serverVariables.Object);

            var contextCore = new Mock<HttpContextCore>();
            contextCore.Setup(c => c.Features).Returns(features.Object);

            var requestCore = new Mock<HttpRequestCore>();
            requestCore.Setup(r => r.HttpContext).Returns(contextCore.Object);

            var request = new HttpRequest(requestCore.Object);

            // Act
            var serverVariables1 = request.ServerVariables;
            var serverVariables2 = request.ServerVariables;

            // Assert
            Assert.Same(serverVariables1, serverVariables2);
            Assert.IsType<ServerVariablesNameValueCollection>(serverVariables1);
        }

        [Fact]
        public void ServerVariableFeatureNotAvailable()
        {
            // Arrange
            var features = new Mock<IFeatureCollection>();

            var contextCore = new Mock<HttpContextCore>();
            contextCore.Setup(c => c.Features).Returns(features.Object);

            var requestCore = new Mock<HttpRequestCore>();
            requestCore.Setup(r => r.HttpContext).Returns(contextCore.Object);

            var request = new HttpRequest(requestCore.Object);

            // Act
            Assert.Throws<PlatformNotSupportedException>(() => request.ServerVariables);
        }

        [Fact]
        public void Form()
        {
            // Arrange
            var form = new Mock<IFormCollection>();
            form.Setup(f => f.Count).Returns(0);

            var requestCore = new Mock<HttpRequestCore>();
            requestCore.Setup(r => r.Form).Returns(form.Object);

            var request = new HttpRequest(requestCore.Object);

            // Act
            var formCollection1 = request.Form;
            var formCollection2 = request.Form;

            // Assert
            Assert.Same(formCollection1, formCollection2);
            Assert.IsType<StringValuesNameValueCollection>(formCollection1);
        }

        [Fact]
        public void Form3Items()
        {
            // Arrange
            var key1 = _fixture.Create<string>();
            var value1 = _fixture.Create<string>();
            var key2 = _fixture.Create<string>();
            var value2 = _fixture.Create<string>();
            var value3 = _fixture.Create<string>();

            var formCollection = new FormCollection(new()
            {
                { key1, value1 },
                { key2, new StringValues(new[] { value2, value3 }) },
            });

            var requestCore = new Mock<HttpRequestCore>();
            requestCore.Setup(r => r.Form).Returns(formCollection);

            var request = new HttpRequest(requestCore.Object);

            // Act
            var form = request.Form;

            // Assert
            Assert.Equal(2, form.Count);
            Assert.Equal(key1, form.GetKey(0));
            Assert.Equal(new[] { value1 }, form.GetValues(0));
            Assert.Equal(value1, form.Get(0));
            Assert.Equal(key2, form.GetKey(1));
            Assert.Equal(new[] { value2, value3 }, form.GetValues(1));
            Assert.Equal($"{value2},{value3}", form.Get(1));
        }

        [Fact]
        public void Query()
        {
            // Arrange
            var query = new Mock<IQueryCollection>();
            query.Setup(f => f.Count).Returns(0);

            var requestCore = new Mock<HttpRequestCore>();
            requestCore.Setup(r => r.Query).Returns(query.Object);

            var request = new HttpRequest(requestCore.Object);

            // Act
            var queryCollection1 = request.QueryString;
            var queryCollection2 = request.QueryString;

            // Assert
            Assert.Same(queryCollection1, queryCollection2);
            Assert.IsType<StringValuesNameValueCollection>(queryCollection1);
        }
    }
}
