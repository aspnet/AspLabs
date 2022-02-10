// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace System.Web
{
    public class HttpRequest
    {
        public string Path => throw new NotImplementedException();

        public NameValueCollection Headers => throw new NotImplementedException();

        public Uri Url => throw new NotImplementedException();

        public string RawUrl => throw new NotImplementedException();

        public string HttpMethod => throw new NotImplementedException();

        public string UserHostAddress => throw new NotImplementedException();

        public string[] UserLanguages => throw new NotImplementedException();

        public string UserAgent => throw new NotImplementedException();

        public string RequestType => HttpMethod;

        public NameValueCollection Form => throw new NotImplementedException();

        public HttpCookieCollection Cookies => throw new NotImplementedException();

        public int ContentLength => throw new NotImplementedException();

        public string ContentType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public Stream InputStream => throw new NotImplementedException();

        public NameValueCollection ServerVariables => throw new NotImplementedException();

        public bool IsSecureConnection => throw new NotImplementedException();

        public NameValueCollection QueryString => throw new NotImplementedException();

        public bool IsLocal => throw new NotImplementedException();

        public string AppRelativeCurrentExecutionFilePath => throw new NotImplementedException();

        public string ApplicationPath => throw new NotImplementedException();

        public Uri UrlReferrer => throw new NotImplementedException();

        public int TotalBytes => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();

        public IIdentity LogonUserIdentity => throw new NotImplementedException();

        public Encoding ContentEncoding => throw new NotImplementedException();

        public string UserHostName => throw new NotImplementedException();

        public HttpBrowserCapabilities Browser => throw new NotImplementedException();

        public byte[] BinaryRead(int count) => throw new NotImplementedException();

        public void Abort() => throw new NotImplementedException();
    }
}
