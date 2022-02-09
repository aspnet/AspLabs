// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.IO;

namespace System.Web
{
    public class HttpResponse
    {
        public int StatusCode
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string StatusDescription
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public NameValueCollection Headers
        {
            get => throw new NotImplementedException();
        }

        public bool TrySkipIisCustomErrors
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string ContentType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public Stream OutputStream => throw new NotImplementedException();

        public HttpCookieCollection Cookies
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool SuppressContent
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string Charset
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public TextWriter Output
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool IsClientConnected => throw new NotImplementedException();

        public void AddHeader(string name, string value) => throw new NotImplementedException();

        public void AppendHeader(string name, string value) => throw new NotImplementedException();

        public void SetCookie(HttpCookie cookie) => throw new NotImplementedException();

        public void End() => throw new NotImplementedException();

        public void Write(char ch) => throw new NotImplementedException();

        public void Write(string s) => throw new NotImplementedException();

        public void Write(object obj) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public void ClearContent() => throw new NotImplementedException();

        public void Abort() => throw new NotImplementedException();
    }
}
