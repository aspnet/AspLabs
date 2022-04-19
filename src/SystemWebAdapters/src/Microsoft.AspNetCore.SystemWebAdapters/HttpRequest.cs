// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.Net.Http.Headers;

namespace System.Web
{
    public class HttpRequest
    {
        private readonly HttpRequestCore _request;

        private RequestHeaders? _typedHeaders;
        private string[]? _userLanguages;
        private NameValueCollection? _headers;
        private NameValueCollection? _serverVariables;
        private NameValueCollection? _form;
        private NameValueCollection? _query;
        private HttpCookieCollection? _cookies;
        private HttpBrowserCapabilities? _browser;

        public HttpRequest(HttpRequestCore request)
        {
            _request = request;
        }

        internal RequestHeaders TypedHeaders => _typedHeaders ??= new(_request.Headers);

        public string? Path => _request.Path.Value;

        public NameValueCollection Headers => _headers ??= _request.Headers.ToNameValueCollection();

        public Uri Url => new(_request.GetEncodedUrl());

        public string? RawUrl => _request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

        public string HttpMethod => _request.Method;

        public string? UserHostAddress => _request.HttpContext.Connection.RemoteIpAddress?.ToString();

        public string[] UserLanguages
        {
            get
            {
                if (_userLanguages is null)
                {
                    var languages = TypedHeaders.AcceptLanguage;
                    var length = languages.Count;

                    if (length == 0)
                    {
                        _userLanguages = Array.Empty<string>();
                    }
                    else
                    {
                        var qualityArray = ArrayPool<StringWithQualityHeaderValue>.Shared.Rent(length);
                        var userLanguages = new string[length];

                        languages.CopyTo(qualityArray, 0);
                        Array.Sort(qualityArray, 0, length, StringWithQualityHeaderValueComparer.Instance);

                        for (var i = 0; i < length; i++)
                        {
                            userLanguages[i] = qualityArray[i].Value.Value;
                        }

                        ArrayPool<StringWithQualityHeaderValue>.Shared.Return(qualityArray);

                        _userLanguages = userLanguages;
                    }
                }

                return _userLanguages;
            }
        }

        public string UserAgent => _request.Headers[HeaderNames.UserAgent];

        public string RequestType => HttpMethod;

        public NameValueCollection Form => _form ??= _request.Form.ToNameValueCollection();

        public HttpCookieCollection Cookies => _cookies ??= new(this);

        public int ContentLength => (int)(_request.ContentLength ?? 0);

        public string? ContentType
        {
            get => _request.ContentType;
            set => _request.ContentType = value;
        }

        public Stream InputStream => _request.Body.CanSeek
            ? _request.Body
            : throw new InvalidOperationException("Input stream must be seekable. Ensure your endpoints are either annotated with BufferRequestStreamAttribute or you've called .RequireRequestStreamBuffering() on them.");

        public NameValueCollection ServerVariables
        {
            get
            {
                if (_serverVariables is null)
                {
                    if (_request.HttpContext.Features.Get<IServerVariablesFeature>() is IServerVariablesFeature feature)
                    {
                        _serverVariables = feature.ToNameValueCollection();
                    }
                    else
                    {
                        throw new PlatformNotSupportedException("IServerVariablesFeature is not available.");
                    }
                }

                return _serverVariables;
            }
        }

        public bool IsSecureConnection => _request.IsHttps;

        public NameValueCollection QueryString => _query ??= _request.Query.ToNameValueCollection();

        public bool IsLocal
        {
            get
            {
                var connectionInfo = _request.HttpContext.Connection;

                // If unknown, assume not local
                if (connectionInfo.RemoteIpAddress is null)
                {
                    return false;
                }

                // Check if localhost
                if (IPAddress.IsLoopback(connectionInfo.RemoteIpAddress))
                {
                    return true;
                }

                return connectionInfo.RemoteIpAddress.Equals(connectionInfo.LocalIpAddress);
            }
        }

        public string AppRelativeCurrentExecutionFilePath => $"~{_request.Path.Value}";

        public string? ApplicationPath => _request.PathBase.Value;

        public Uri? UrlReferrer => TypedHeaders.Referer;

        public int TotalBytes => (int)InputStream.Length;

        public bool IsAuthenticated => LogonUserIdentity?.IsAuthenticated ?? false;

        public IIdentity? LogonUserIdentity => _request.HttpContext.User.Identity;

        public Encoding? ContentEncoding => TypedHeaders.ContentType?.Encoding;

        public string? UserHostName => _request.HttpContext.Connection.RemoteIpAddress?.ToString();

        public HttpBrowserCapabilities Browser => _browser ??= new();

        public byte[] BinaryRead(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            var buffer = new byte[count];
            var read = InputStream.Read(buffer);

            if (read == 0)
            {
                return Array.Empty<byte>();
            }

            if (read < count)
            {
                Array.Resize(ref buffer, read);
            }

            return buffer;
        }

        public void Abort() => _request.HttpContext.Abort();

        [return: NotNullIfNotNull("request")]
        public static implicit operator HttpRequest?(HttpRequestCore? request) => request.GetAdapter();

        [return: NotNullIfNotNull("request")]
        public static implicit operator HttpRequestCore?(HttpRequest? request) => request?._request;

        private class StringWithQualityHeaderValueComparer : IComparer<StringWithQualityHeaderValue>
        {
            public static StringWithQualityHeaderValueComparer Instance { get; } = new();

            public int Compare(StringWithQualityHeaderValue? x, StringWithQualityHeaderValue? y)
            {
                var xValue = x?.Quality ?? 1;
                var yValue = y?.Quality ?? 1;

                return yValue.CompareTo(xValue);
            }
        }
    }
}
