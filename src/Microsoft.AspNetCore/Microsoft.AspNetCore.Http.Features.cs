namespace Microsoft.AspNetCore.Http
{
    public partial class CookieOptions
    {
        public CookieOptions() { }
        public string Domain { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Nullable<System.DateTimeOffset> Expires { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool HttpOnly { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string Path { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool Secure { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial interface IFormCollection : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>, System.Collections.IEnumerable
    {
        int Count { get; }
        Microsoft.AspNetCore.Http.IFormFileCollection Files { get; }
        Microsoft.Extensions.Primitives.StringValues this[string key] { get; }
        System.Collections.Generic.ICollection<string> Keys { get; }
        bool ContainsKey(string key);
        bool TryGetValue(string key, out Microsoft.Extensions.Primitives.StringValues value);
    }
    public partial interface IFormFile
    {
        string ContentDisposition { get; }
        string ContentType { get; }
        string FileName { get; }
        Microsoft.AspNetCore.Http.IHeaderDictionary Headers { get; }
        long Length { get; }
        string Name { get; }
        void CopyTo(System.IO.Stream target);
        System.Threading.Tasks.Task CopyToAsync(System.IO.Stream target, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
        System.IO.Stream OpenReadStream();
    }
    public partial interface IFormFileCollection : System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Http.IFormFile>, System.Collections.Generic.IReadOnlyCollection<Microsoft.AspNetCore.Http.IFormFile>, System.Collections.Generic.IReadOnlyList<Microsoft.AspNetCore.Http.IFormFile>, System.Collections.IEnumerable
    {
        Microsoft.AspNetCore.Http.IFormFile this[string name] { get; }
        Microsoft.AspNetCore.Http.IFormFile GetFile(string name);
        System.Collections.Generic.IReadOnlyList<Microsoft.AspNetCore.Http.IFormFile> GetFiles(string name);
    }
    public partial interface IHeaderDictionary : System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>, System.Collections.Generic.IDictionary<string, Microsoft.Extensions.Primitives.StringValues>, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>, System.Collections.IEnumerable
    {
        new Microsoft.Extensions.Primitives.StringValues this[string key] { get; set; }
    }
    public partial interface IQueryCollection : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>, System.Collections.IEnumerable
    {
        int Count { get; }
        Microsoft.Extensions.Primitives.StringValues this[string key] { get; }
        System.Collections.Generic.ICollection<string> Keys { get; }
        bool ContainsKey(string key);
        bool TryGetValue(string key, out Microsoft.Extensions.Primitives.StringValues value);
    }
    public partial interface IRequestCookieCollection : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Collections.IEnumerable
    {
        int Count { get; }
        string this[string key] { get; }
        System.Collections.Generic.ICollection<string> Keys { get; }
        bool ContainsKey(string key);
        bool TryGetValue(string key, out string value);
    }
    public partial interface IResponseCookies
    {
        void Append(string key, string value);
        void Append(string key, string value, Microsoft.AspNetCore.Http.CookieOptions options);
        void Delete(string key);
        void Delete(string key, Microsoft.AspNetCore.Http.CookieOptions options);
    }
    public partial interface ISession
    {
        string Id { get; }
        bool IsAvailable { get; }
        System.Collections.Generic.IEnumerable<string> Keys { get; }
        void Clear();
        System.Threading.Tasks.Task CommitAsync();
        System.Threading.Tasks.Task LoadAsync();
        void Remove(string key);
        void Set(string key, byte[] value);
        bool TryGetValue(string key, out byte[] value);
    }
    public partial class WebSocketAcceptContext
    {
        public WebSocketAcceptContext() { }
        public virtual string SubProtocol { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
}
namespace Microsoft.AspNetCore.Http.Features
{
    public partial class FeatureCollection : Microsoft.AspNetCore.Http.Features.IFeatureCollection, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Type, object>>, System.Collections.IEnumerable
    {
        public FeatureCollection() { }
        public FeatureCollection(Microsoft.AspNetCore.Http.Features.IFeatureCollection defaults) { }
        public bool IsReadOnly { get { throw null; } }
        public object this[System.Type key] { get { throw null; } set { } }
        public virtual int Revision { get { throw null; } }
        public TFeature Get<TFeature>() { throw null; }
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.Type, object>> GetEnumerator() { throw null; }
        public void Set<TFeature>(TFeature instance) { }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct FeatureReference<T>
    {
        public static readonly Microsoft.AspNetCore.Http.Features.FeatureReference<T> Default;
        public T Fetch(Microsoft.AspNetCore.Http.Features.IFeatureCollection features) { throw null; }
        public T Update(Microsoft.AspNetCore.Http.Features.IFeatureCollection features, T feature) { throw null; }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct FeatureReferences<TCache>
    {
        public TCache Cache;
        public FeatureReferences(Microsoft.AspNetCore.Http.Features.IFeatureCollection collection) { throw null;}
        public Microsoft.AspNetCore.Http.Features.IFeatureCollection Collection { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public int Revision { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public TFeature Fetch<TFeature>(ref TFeature cached, System.Func<Microsoft.AspNetCore.Http.Features.IFeatureCollection, TFeature> factory) where TFeature : class { throw null; }
        public TFeature Fetch<TFeature, TState>(ref TFeature cached, TState state, System.Func<TState, TFeature> factory) where TFeature : class { throw null; }
    }
    public partial interface IFeatureCollection : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Type, object>>, System.Collections.IEnumerable
    {
        bool IsReadOnly { get; }
        object this[System.Type key] { get; set; }
        int Revision { get; }
        TFeature Get<TFeature>();
        void Set<TFeature>(TFeature instance);
    }
    public partial interface IFormFeature
    {
        Microsoft.AspNetCore.Http.IFormCollection Form { get; set; }
        bool HasFormContentType { get; }
        Microsoft.AspNetCore.Http.IFormCollection ReadForm();
        System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IFormCollection> ReadFormAsync(System.Threading.CancellationToken cancellationToken);
    }
    public partial interface IHttpBufferingFeature
    {
        void DisableRequestBuffering();
        void DisableResponseBuffering();
    }
    public partial interface IHttpConnectionFeature
    {
        string ConnectionId { get; set; }
        System.Net.IPAddress LocalIpAddress { get; set; }
        int LocalPort { get; set; }
        System.Net.IPAddress RemoteIpAddress { get; set; }
        int RemotePort { get; set; }
    }
    public partial interface IHttpRequestFeature
    {
        System.IO.Stream Body { get; set; }
        Microsoft.AspNetCore.Http.IHeaderDictionary Headers { get; set; }
        string Method { get; set; }
        string Path { get; set; }
        string PathBase { get; set; }
        string Protocol { get; set; }
        string QueryString { get; set; }
        string RawTarget { get; set; }
        string Scheme { get; set; }
    }
    public partial interface IHttpRequestIdentifierFeature
    {
        string TraceIdentifier { get; set; }
    }
    public partial interface IHttpRequestLifetimeFeature
    {
        System.Threading.CancellationToken RequestAborted { get; set; }
        void Abort();
    }
    public partial interface IHttpResponseFeature
    {
        System.IO.Stream Body { get; set; }
        bool HasStarted { get; }
        Microsoft.AspNetCore.Http.IHeaderDictionary Headers { get; set; }
        string ReasonPhrase { get; set; }
        int StatusCode { get; set; }
        void OnCompleted(System.Func<object, System.Threading.Tasks.Task> callback, object state);
        void OnStarting(System.Func<object, System.Threading.Tasks.Task> callback, object state);
    }
    public partial interface IHttpSendFileFeature
    {
        System.Threading.Tasks.Task SendFileAsync(string path, long offset, System.Nullable<long> count, System.Threading.CancellationToken cancellation);
    }
    public partial interface IHttpUpgradeFeature
    {
        bool IsUpgradableRequest { get; }
        System.Threading.Tasks.Task<System.IO.Stream> UpgradeAsync();
    }
    public partial interface IHttpWebSocketFeature
    {
        bool IsWebSocketRequest { get; }
        System.Threading.Tasks.Task<System.Net.WebSockets.WebSocket> AcceptAsync(Microsoft.AspNetCore.Http.WebSocketAcceptContext context);
    }
    public partial interface IItemsFeature
    {
        System.Collections.Generic.IDictionary<object, object> Items { get; set; }
    }
    public partial interface IQueryFeature
    {
        Microsoft.AspNetCore.Http.IQueryCollection Query { get; set; }
    }
    public partial interface IRequestCookiesFeature
    {
        Microsoft.AspNetCore.Http.IRequestCookieCollection Cookies { get; set; }
    }
    public partial interface IResponseCookiesFeature
    {
        Microsoft.AspNetCore.Http.IResponseCookies Cookies { get; }
    }
    public partial interface IServiceProvidersFeature
    {
        System.IServiceProvider RequestServices { get; set; }
    }
    public partial interface ISessionFeature
    {
        Microsoft.AspNetCore.Http.ISession Session { get; set; }
    }
    public partial interface ITlsConnectionFeature
    {
        System.Security.Cryptography.X509Certificates.X509Certificate2 ClientCertificate { get; set; }
        System.Threading.Tasks.Task<System.Security.Cryptography.X509Certificates.X509Certificate2> GetClientCertificateAsync(System.Threading.CancellationToken cancellationToken);
    }
    public partial interface ITlsTokenBindingFeature
    {
        byte[] GetProvidedTokenBindingId();
        byte[] GetReferredTokenBindingId();
    }
}
namespace Microsoft.AspNetCore.Http.Features.Authentication
{
    public partial class AuthenticateContext
    {
        public AuthenticateContext(string authenticationScheme) { }
        public bool Accepted { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string AuthenticationScheme { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IDictionary<string, object> Description { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Exception Error { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Security.Claims.ClaimsPrincipal Principal { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IDictionary<string, string> Properties { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public virtual void Authenticated(System.Security.Claims.ClaimsPrincipal principal, System.Collections.Generic.IDictionary<string, string> properties, System.Collections.Generic.IDictionary<string, object> description) { }
        public virtual void Failed(System.Exception error) { }
        public virtual void NotAuthenticated() { }
    }
    public enum ChallengeBehavior
    {
        Automatic = 0,
        Forbidden = 2,
        Unauthorized = 1,
    }
    public partial class ChallengeContext
    {
        public ChallengeContext(string authenticationScheme) { }
        public ChallengeContext(string authenticationScheme, System.Collections.Generic.IDictionary<string, string> properties, Microsoft.AspNetCore.Http.Features.Authentication.ChallengeBehavior behavior) { }
        public bool Accepted { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string AuthenticationScheme { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Http.Features.Authentication.ChallengeBehavior Behavior { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IDictionary<string, string> Properties { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public void Accept() { }
    }
    public partial class DescribeSchemesContext
    {
        public DescribeSchemesContext() { }
        public System.Collections.Generic.IEnumerable<System.Collections.Generic.IDictionary<string, object>> Results { get { throw null; } }
        public void Accept(System.Collections.Generic.IDictionary<string, object> description) { }
    }
    public partial interface IAuthenticationHandler
    {
        System.Threading.Tasks.Task AuthenticateAsync(Microsoft.AspNetCore.Http.Features.Authentication.AuthenticateContext context);
        System.Threading.Tasks.Task ChallengeAsync(Microsoft.AspNetCore.Http.Features.Authentication.ChallengeContext context);
        void GetDescriptions(Microsoft.AspNetCore.Http.Features.Authentication.DescribeSchemesContext context);
        System.Threading.Tasks.Task SignInAsync(Microsoft.AspNetCore.Http.Features.Authentication.SignInContext context);
        System.Threading.Tasks.Task SignOutAsync(Microsoft.AspNetCore.Http.Features.Authentication.SignOutContext context);
    }
    public partial interface IHttpAuthenticationFeature
    {
        Microsoft.AspNetCore.Http.Features.Authentication.IAuthenticationHandler Handler { get; set; }
        System.Security.Claims.ClaimsPrincipal User { get; set; }
    }
    public partial class SignInContext
    {
        public SignInContext(string authenticationScheme, System.Security.Claims.ClaimsPrincipal principal, System.Collections.Generic.IDictionary<string, string> properties) { }
        public bool Accepted { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string AuthenticationScheme { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Security.Claims.ClaimsPrincipal Principal { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IDictionary<string, string> Properties { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public void Accept() { }
    }
    public partial class SignOutContext
    {
        public SignOutContext(string authenticationScheme, System.Collections.Generic.IDictionary<string, string> properties) { }
        public bool Accepted { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string AuthenticationScheme { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IDictionary<string, string> Properties { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public void Accept() { }
    }
}
