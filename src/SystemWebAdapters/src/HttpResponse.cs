// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Web.Adapters;
using System.Web.Internal;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace System.Web
{
    public class HttpResponse
    {
        private const string NoContentTypeMessage = "No content type declared";

        private readonly HttpResponseCore _response;

        private NameValueCollection? _headers;
        private ResponseHeaders? _typedHeaders;
        private IBufferedResponseFeature? _bufferedFeature;
        private TextWriter? _writer;
        private HttpCookieCollection? _cookies;

        public HttpResponse(HttpResponseCore response)
        {
            _response = response;
        }

        private IBufferedResponseFeature BufferedFeature => _bufferedFeature ??= _response.HttpContext.Features.Get<IBufferedResponseFeature>()
            ?? throw new InvalidOperationException("Response buffering must be enabled on this endpoint for this feature via the IBufferResponseStreamMetadata metadata item");

        internal ResponseHeaders TypedHeaders => _typedHeaders ??= new(_response.Headers);

        public int StatusCode
        {
            get => _response.StatusCode;
            set => _response.StatusCode = value;
        }

        public string StatusDescription
        {
            get => _response.HttpContext.Features.GetRequired<IHttpResponseFeature>().ReasonPhrase ?? ReasonPhrases.GetReasonPhrase(_response.StatusCode);
            set => _response.HttpContext.Features.GetRequired<IHttpResponseFeature>().ReasonPhrase = value;
        }

        public NameValueCollection Headers => _headers ??= _response.Headers.ToNameValueCollection();

        public bool TrySkipIisCustomErrors
        {
            get => _response.HttpContext.Features.GetRequired<IStatusCodePagesFeature>().Enabled;
            set => _response.HttpContext.Features.GetRequired<IStatusCodePagesFeature>().Enabled = value;
        }

        public Stream OutputStream => _response.Body;

        public HttpCookieCollection Cookies
        {
            get => _cookies ??= new(this);
        }

        public bool SuppressContent
        {
            get => BufferedFeature.SuppressContent;
            set => BufferedFeature.SuppressContent = value;
        }

        public Encoding ContentEncoding
        {
            get => TypedHeaders.ContentType?.Encoding ?? Encoding.UTF8;
            set
            {
                if (TypedHeaders.ContentType is { } contentType)
                {
                    if (contentType.Encoding == value)
                    {
                        return;
                    }

                    contentType.Encoding = value;
                    TypedHeaders.ContentType = contentType;

                    // Reset the writer for change in encoding
                    _writer = null;
                }
                else
                {
                    throw new InvalidOperationException(NoContentTypeMessage);
                }
            }
        }

        public string? ContentType
        {
            get => TypedHeaders.ContentType?.MediaType.ToString();
            set
            {
                if (TypedHeaders.ContentType is { } contentType)
                {
                    contentType.MediaType = value;
                    TypedHeaders.ContentType = contentType;
                }
                else
                {
                    TypedHeaders.ContentType = new(value);
                }
            }
        }

        public string Charset
        {
            get => TypedHeaders.ContentType?.Charset.Value ?? Encoding.UTF8.WebName;
            set
            {
                if (TypedHeaders.ContentType is { } contentType)
                {
                    contentType.Charset = value;
                    TypedHeaders.ContentType = contentType;
                }
                else
                {
                    throw new InvalidOperationException(NoContentTypeMessage);
                }
            }
        }

        public TextWriter Output
        {
            get
            {
                if (_writer is null)
                {
                    _writer = new StreamWriter(_response.Body, ContentEncoding, leaveOpen: true)
                    {
                        AutoFlush = true,
                    };
                }

                return _writer;
            }

            set => _writer = value;
        }

        public bool IsClientConnected => !_response.HttpContext.RequestAborted.IsCancellationRequested;

        public void AddHeader(string name, string value) => _response.Headers.Add(name, value);

        public void AppendHeader(string name, string value)
        {
            if (_response.Headers.TryGetValue(name, out var existing))
            {
                _response.Headers.Add(name, StringValues.Concat(existing, value));
            }
            else
            {
                _response.Headers.Add(name, value);
            }
        }

        public void SetCookie(HttpCookie cookie) => Cookies.Set(cookie);

        public void End() => BufferedFeature.End();

        public void Write(char ch) => Output.Write(ch);

        public void Write(string s) => Output.Write(s);

        public void Write(object obj) => Output.Write(obj);

        public void Clear()
        {
            _response.Clear();
            ClearContent();
        }

        public void ClearContent()
        {
            if (_response.Body.CanSeek)
            {
                _response.Body.SetLength(0);
            }
            else
            {
                BufferedFeature.ClearContent();
            }
        }

        public void Abort() => _response.HttpContext.Abort();

        [return: NotNullIfNotNull("response")]
        public static implicit operator HttpResponse?(HttpResponseCore? response) => response?.GetAdapter();

        [return: NotNullIfNotNull("response")]
        public static implicit operator HttpResponseCore?(HttpResponse? response) => response?._response;
    }
}
