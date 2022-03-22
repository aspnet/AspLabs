// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Web.Adapters;
using System.Web.Internal;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace System.Web
{
    public class HttpResponse
    {
        private readonly HttpResponseCore _response;

        private StringValuesNameValueCollection? _headers;
        private ResponseHeaders? _typedHeaders;
        private IBufferedResponseFeature? _bufferedFeature;
        private TextWriter? _writer;

        public HttpResponse(HttpResponseCore response)
        {
            _response = response;
        }

        private IBufferedResponseFeature BufferedFeature => _bufferedFeature ??= _response.HttpContext.Features.Get<IBufferedResponseFeature>() ?? throw new InvalidOperationException("Response buffering must be enabled on this endpoint for this feature via the IBufferResponseStreamMetadata metadata item");

        private ResponseHeaders TypedHeaders => _typedHeaders ??= new(_response.Headers);

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

        public NameValueCollection Headers => _headers ??= new(_response.Headers);

        public bool TrySkipIisCustomErrors
        {
            get => _response.HttpContext.Features.GetRequired<IStatusCodePagesFeature>().Enabled;
            set => _response.HttpContext.Features.GetRequired<IStatusCodePagesFeature>().Enabled = value;
        }

        public string ContentType
        {
            get => _response.ContentType;
            set => _response.ContentType = value;
        }

        public Stream OutputStream => BufferedFeature.Stream;

        public HttpCookieCollection Cookies
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
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
                var contentType = TypedHeaders.ContentType;

                if (contentType is null)
                {
                    contentType = new(value.WebName);
                    TypedHeaders.ContentType = contentType;
                }
                else
                {
                    contentType.Encoding = value;
                }
            }
        }

        public string Charset
        {
            get => TypedHeaders.ContentType?.Charset.Value ?? Encoding.UTF8.WebName;
            set
            {
                var contentType = TypedHeaders.ContentType;

                if (contentType is null)
                {
                    contentType = new(value);
                    TypedHeaders.ContentType = contentType;
                }
                else
                {
                    contentType.Charset = value;
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

        public bool IsClientConnected => _response.HttpContext.RequestAborted.IsCancellationRequested;

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
        public void SetCookie(HttpCookie cookie) => throw new NotImplementedException();

        // On .NET Framework, this throws a ThreadAbortException. The goal is to not allow any more output after this, so this flag does that rather than throwing as that ends up with a much larger perf overhead.
        // Any additional writes will end up throwing an exception since it's marked as ended.
        public void End() => BufferedFeature.IsEnded = true;

        public void Write(char ch) => Output.Write(ch);

        public void Write(string s) => Output.Write(s);

        public void Write(object obj) => Output.Write(obj);

        public void Clear() => BufferedFeature.Clear();

        public void ClearContent() => BufferedFeature.ClearContent();

        public void Abort() => _response.HttpContext.Abort();

        [return: NotNullIfNotNull("response")]
        public static implicit operator HttpResponse?(HttpResponseCore? response) => response?.GetAdapter();

        [return: NotNullIfNotNull("response")]
        public static implicit operator HttpResponseCore?(HttpResponse? response) => response?._response;
    }
}
