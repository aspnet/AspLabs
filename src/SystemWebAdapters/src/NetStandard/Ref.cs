// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web
{
    public partial class HttpBrowserCapabilities
    {
        public HttpBrowserCapabilities() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpContext : System.IServiceProvider
    {
        internal HttpContext() { }
        public System.Web.Caching.Cache Cache { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public static System.Web.HttpContext Current { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.IDictionary Items { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpRequest Request { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpResponse Response { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpServerUtility Server { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.SessionState.HttpSessionState Session { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Security.Principal.IPrincipal User { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public object GetService(System.Type service) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpContextBase : System.IServiceProvider
    {
        protected HttpContextBase() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual System.Collections.IDictionary Items { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpRequestBase Request { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpResponseBase Response { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpServerUtilityBase Server { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpSessionStateBase Session { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Security.Principal.IPrincipal User { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual object GetService(System.Type serviceType) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpContextWrapper : System.Web.HttpContextBase
    {
        public HttpContextWrapper(System.Web.HttpContext httpContext) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override System.Collections.IDictionary Items { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpRequestBase Request { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpResponseBase Response { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpSessionStateBase Session { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Security.Principal.IPrincipal User { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
    }
    public partial class HttpCookie
    {
        public HttpCookie() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public string Domain { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.DateTime Expires { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool HttpOnly { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string Name { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.SameSiteMode SameSite { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool Secure { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string Value { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Values { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
    }
    public sealed partial class HttpCookieCollection : System.Collections.Specialized.NameObjectCollectionBase
    {
        public HttpCookieCollection() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public string[] AllKeys { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpCookie this[int index] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpCookie this[string name] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public void Add(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public System.Web.HttpCookie Get(int index) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public System.Web.HttpCookie Get(string name) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public string GetKey(int index) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Remove(string name) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Set(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpRequest
    {
        internal HttpRequest() { }
        public string ApplicationPath { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string AppRelativeCurrentExecutionFilePath { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpBrowserCapabilities Browser { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public int ContentLength { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string ContentType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Form { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string HttpMethod { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool IsAuthenticated { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool IsLocal { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool IsSecureConnection { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Security.Principal.IIdentity LogonUserIdentity { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string Path { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection QueryString { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string RawUrl { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string RequestType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection ServerVariables { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public int TotalBytes { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Uri Url { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Uri UrlReferrer { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string UserAgent { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string UserHostAddress { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string UserHostName { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string[] UserLanguages { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public void Abort() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public byte[] BinaryRead(int count) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public abstract partial class HttpRequestBase
    {
        protected HttpRequestBase() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public string ApplicationPath { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string AppRelativeCurrentExecutionFilePath { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpBrowserCapabilities Browser { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual int ContentLength { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string ContentType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string HttpMethod { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual bool IsAuthenticated { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual bool IsLocal { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool IsSecureConnection { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Security.Principal.IIdentity LogonUserIdentity { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string Path { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection QueryString { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string RawUrl { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string RequestType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection ServerVariables { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual int TotalBytes { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Uri Url { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Uri UrlReferrer { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string UserAgent { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string UserHostAddress { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string UserHostName { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string[] UserLanguages { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual void Abort() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual byte[] BinaryRead(int count) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpRequestWrapper : System.Web.HttpRequestBase
    {
        public HttpRequestWrapper(System.Web.HttpRequest request) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override int ContentLength { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string ContentType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string HttpMethod { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override bool IsAuthenticated { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override bool IsLocal { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Security.Principal.IIdentity LogonUserIdentity { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string Path { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection QueryString { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string RawUrl { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string RequestType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override int TotalBytes { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Uri Url { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Uri UrlReferrer { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string UserAgent { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string UserHostAddress { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string UserHostName { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string[] UserLanguages { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override void Abort() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override byte[] BinaryRead(int count) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpResponse
    {
        internal HttpResponse() { }
        public string Charset { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string ContentType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool IsClientConnected { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.IO.TextWriter Output { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.IO.Stream OutputStream { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public int StatusCode { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string StatusDescription { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool SuppressContent { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool TrySkipIisCustomErrors { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public void Abort() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void AddHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void AppendHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void ClearContent() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void End() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void SetCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Write(char ch) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Write(object obj) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Write(string s) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpResponseBase
    {
        public HttpResponseBase() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public string Charset { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string ContentType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual bool IsClientConnected { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.IO.TextWriter Output { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.IO.Stream OutputStream { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual int StatusCode { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string StatusDescription { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual bool SuppressContent { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual bool TrySkipIisCustomErrors { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual void Abort() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void AddHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void AppendHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void ClearContent() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void End() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void SetCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Write(char ch) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Write(object obj) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Write(string s) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpResponseWrapper : System.Web.HttpResponseBase
    {
        public HttpResponseWrapper(System.Web.HttpResponse response) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override string ContentType { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override bool IsClientConnected { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.IO.TextWriter Output { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override System.IO.Stream OutputStream { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override int StatusCode { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string StatusDescription { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override bool SuppressContent { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override bool TrySkipIisCustomErrors { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override void Abort() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void AddHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void SetCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void Write(char ch) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void Write(object obj) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void Write(string s) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpServerUtility
    {
        internal HttpServerUtility() { }
        public string MachineName { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public void ClearError() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public System.Exception GetLastError() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public string MapPath(string path) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public static byte[] UrlTokenDecode(string input) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public static string UrlTokenEncode(byte[] input) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpServerUtilityBase
    {
        public HttpServerUtilityBase() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual string MachineName { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual void ClearError() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual System.Exception GetLastError() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual string MapPath(string path) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual byte[] UrlTokenDecode(string input) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual string UrlTokenEncode(byte[] input) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpSessionStateBase
    {
        public HttpSessionStateBase() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual int Count { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual bool IsReadOnly { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual object this[string name] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string SessionID { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual int TimeOut { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual void Abandon() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Clear() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Remove(string name) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void RemoveAll() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpSessionStateWrapper : System.Web.HttpSessionStateBase
    {
        public HttpSessionStateWrapper(System.Web.SessionState.HttpSessionState session) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override int Count { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override bool IsReadOnly { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override object this[string name] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override string SessionID { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override int TimeOut { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public override void Abandon() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void Clear() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void Remove(string name) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public override void RemoveAll() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public enum SameSiteMode
    {
    }
}
namespace System.Web.Caching
{
    public partial class Cache
    {
        public static readonly System.DateTime NoAbsoluteExpiration;
        public static readonly System.TimeSpan NoSlidingExpiration;
        public Cache() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public object this[string key] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public object Get(string key) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Insert(string key, object value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public object Remove(string key) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
}
namespace System.Web.SessionState
{
    public partial class HttpSessionState
    {
        internal HttpSessionState() { }
        public int Count { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool IsReadOnly { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public object this[string name] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string SessionID { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public int TimeOut { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public void Abandon() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Remove(string name) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void RemoveAll() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial interface IHttpSessionContainer
    {
        int Count { get; }
        bool IsReadOnly { get; }
        object this[string name] { get; set; }
        string SessionID { get; }
        int TimeOut { get; set; }
        void Abandon();
        void Add(string name, object value);
        void Clear();
        void Remove(string name);
    }
}
