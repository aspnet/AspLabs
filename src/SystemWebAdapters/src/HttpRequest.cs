// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Net.Http.Headers;

namespace System.Web
{
    public class HttpRequest
    {
        private readonly HttpRequestCore _request;

        private RequestHeaders? _typedHeaders;
        private string[]? _userLanguages;

        public HttpRequest(HttpRequestCore request)
        {
            _request = request;
        }

        public string Path => _request.Path.Value;

        public NameValueCollection Headers => throw new NotImplementedException();

        public Uri Url => new(_request.GetEncodedUrl());

        public string RawUrl => _request.HttpContext.GetMetadata().RawUrl ?? throw new InvalidOperationException(LocalizedStrings.RawUrlNotAvailable);

        public string HttpMethod => _request.Method;

        public string UserHostAddress => _request.Host.Value;

        public string[] UserLanguages
        {
            get
            {
                if (_userLanguages is null)
                {
                    _userLanguages = TypedHeaders.AcceptLanguage
                         ?.OrderByDescending(static lang => lang.Quality ?? 1)
                         .Select(static lang => lang.Value.ToString())
                         .ToArray() ?? Array.Empty<string>();
                }

                return _userLanguages;
            }
        }

        public string UserAgent => _request.Headers[HeaderNames.UserAgent];

        public string RequestType => HttpMethod;

        public NameValueCollection Form => throw new NotImplementedException();

        public HttpCookieCollection Cookies => throw new NotImplementedException();

        public int ContentLength => (int)(_request.ContentLength ?? 0);

        public string ContentType
        {
            get => _request.ContentType;
            set => _request.ContentType = value;
        }

        public Stream InputStream => throw new NotImplementedException();

        public NameValueCollection ServerVariables => throw new NotImplementedException();

        public bool IsSecureConnection => _request.IsHttps;

        public NameValueCollection QueryString => throw new NotImplementedException();

        public bool IsLocal
        {
            get
            {
                var connectionInfo = _request.HttpContext.Connection;
                var local = connectionInfo.LocalIpAddress;
                var remote = connectionInfo.RemoteIpAddress;

                if (local is null && remote is null)
                {
                    return true;
                }

                if (remote is not null)
                {
                    if (local is not null)
                    {
                        if (Equals(local, remote) ||
                            (IPAddress.IsLoopback(local) && IPAddress.IsLoopback(remote)))
                        {
                            return true;
                        }
                    }
                    else if (IPAddress.IsLoopback(remote))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Same as CurrentExecutionFilePath, but made relative to the application root, so it is application-agnostic.
        /// </summary>
        /// <remarks>
        /// See https://stackoverflow.com/questions/9701309/get-app-relative-url-from-request-url-absolutepath.
        /// </remarks>
        public string AppRelativeCurrentExecutionFilePath => $"~{_request.Path}";

        public string ApplicationPath => _request.PathBase;

        public Uri UrlReferrer => TypedHeaders.Referer;

        public int TotalBytes => (int)_request.ContentLength.GetValueOrDefault();

        public bool IsAuthenticated => LogonUserIdentity.IsAuthenticated;

        public IIdentity LogonUserIdentity => _request.HttpContext.User.Identity;

        public Encoding? ContentEncoding => TypedHeaders.ContentType?.Encoding;

        public string? UserHostName => _request.HttpContext.Connection.RemoteIpAddress?.ToString();

        public HttpBrowserCapabilities Browser => throw new NotImplementedException();

        public byte[] BinaryRead(int count) => throw new NotImplementedException();

        public void Abort() => _request.HttpContext.Abort();

        private RequestHeaders TypedHeaders
        {
            get
            {
                if (_typedHeaders is null)
                {
                    _typedHeaders = new(_request.Headers);
                }

                return _typedHeaders;
            }
        }

        [return: NotNullIfNotNull("request")]
        public static implicit operator HttpRequest?(HttpRequestCore? request) => request.GetAdapter();

        [return: NotNullIfNotNull("request")]
        public static implicit operator HttpRequestCore?(HttpRequest? request) => request?._request;
    }
}
