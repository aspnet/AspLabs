// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.IO;

namespace System.Web
{
    public class HttpResponseWrapper : HttpResponseBase
    {
        private readonly HttpResponse _response;

        public HttpResponseWrapper(HttpResponse response)
        {
            _response = response;
        }

        public override void Abort() => _response.Abort();

        public override void AddHeader(string name, string value) => _response.AddHeader(name, value);

        public override string ContentType
        {
            get => _response.ContentType;
            set => _response.ContentType = value;
        }

        public override HttpCookieCollection Cookies
        {
            get => _response.Cookies;
            set => _response.Cookies = value;
        }

        public override NameValueCollection Headers => _response.Headers;

        public override bool IsClientConnected => _response.IsClientConnected;

        public override TextWriter Output
        {
            get => _response.Output;
            set => _response.Output = value;
        }

        public override Stream OutputStream => _response.OutputStream;

        public override void SetCookie(HttpCookie cookie) => _response.SetCookie(cookie);

        public override int StatusCode
        {
            get => _response.StatusCode;
            set => _response.StatusCode = value;
        }

        public override string StatusDescription
        {
            get => _response.StatusDescription;
            set => _response.StatusDescription = value;
        }

        public override bool SuppressContent
        {
            get => _response.SuppressContent;
            set => _response.SuppressContent = value;
        }

        public override bool TrySkipIisCustomErrors
        {
            get => _response.TrySkipIisCustomErrors;
            set => _response.TrySkipIisCustomErrors = value;
        }

        public override void Write(char ch) => _response.Write(ch);

        public override void Write(object obj) => _response.Write(obj);

        public override void Write(string s) => _response.Write(s);
    }
}
