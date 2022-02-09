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
        public object GetService(System.Type serviceType) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpContextBase : System.IServiceProvider
    {
        protected HttpContextBase() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual System.Collections.IDictionary Items { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpRequestBase Request { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpResponseBase Response { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpServerUtilityBase Server { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpSessionStateBase Session { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Security.Principal.IPrincipal User { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual object GetService(System.Type serviceType) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
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
        public HttpRequest() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
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
    public partial class HttpResponse
    {
        public HttpResponse() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
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
    public partial class HttpServerUtility
    {
        public HttpServerUtility() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
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
    public abstract partial class HttpSessionStateBase : System.Collections.ICollection, System.Collections.IEnumerable
    {
        protected HttpSessionStateBase() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual System.Web.HttpSessionStateBase Contents { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual int Count { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual bool IsReadOnly { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual bool IsSynchronized { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual object this[int index] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual object this[string name] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameObjectCollectionBase.KeysCollection Keys { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual string SessionID { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual object SyncRoot { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual int Timeout { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public virtual void Abandon() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Clear() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void CopyTo(System.Array array, int index) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual System.Collections.IEnumerator GetEnumerator() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void Remove(string name) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void RemoveAll() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public virtual void RemoveAt(int index) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
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
    public partial class HttpSessionState : System.Collections.ICollection, System.Collections.IEnumerable
    {
        public HttpSessionState() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public System.Web.HttpSessionStateBase Contents { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public int Count { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public bool IsSynchronized { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public object this[int index] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public object this[string name] { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameObjectCollectionBase.KeysCollection Keys { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public string SessionID { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public object SyncRoot { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public int Timeout { get { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");} }
        public void Abandon() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void CopyTo(System.Array array, int index) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public System.Collections.IEnumerator GetEnumerator() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void Remove(string name) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void RemoveAll() { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
        public void RemoveAt(int index) { throw new System.PlatformNotSupportedException("Only support when running on ASP.NET Core or System.Web");}
    }
}
