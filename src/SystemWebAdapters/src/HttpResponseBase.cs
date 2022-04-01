// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace System.Web
{
    public class HttpResponseBase
    {
        public virtual int StatusCode
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual string StatusDescription
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual NameValueCollection Headers
        {
            get => throw new NotImplementedException();
        }

        public virtual bool TrySkipIisCustomErrors
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual string? ContentType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual Encoding ContentEncoding
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual Stream OutputStream => throw new NotImplementedException();

        public virtual HttpCookieCollection Cookies
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual bool SuppressContent
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string Charset
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual TextWriter Output
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual bool IsClientConnected => throw new NotImplementedException();

        public virtual void AddHeader(string name, string value) => throw new NotImplementedException();

        public void AppendHeader(string name, string value) => throw new NotImplementedException();

        public virtual void SetCookie(HttpCookie cookie) => throw new NotImplementedException();

        public void End() => throw new NotImplementedException();

        public virtual void Write(char ch) => throw new NotImplementedException();

        public virtual void Write(string s) => throw new NotImplementedException();

        public virtual void Write(object obj) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public void ClearContent() => throw new NotImplementedException();

        public virtual void Abort() => throw new NotImplementedException();
    }
}
