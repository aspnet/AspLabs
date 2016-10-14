namespace Microsoft.AspNetCore.Builder
{
    public partial interface IApplicationBuilder
    {
        System.IServiceProvider ApplicationServices { get; set; }
        System.Collections.Generic.IDictionary<string, object> Properties { get; }
        Microsoft.AspNetCore.Http.Features.IFeatureCollection ServerFeatures { get; }
        Microsoft.AspNetCore.Http.RequestDelegate Build();
        Microsoft.AspNetCore.Builder.IApplicationBuilder New();
        Microsoft.AspNetCore.Builder.IApplicationBuilder Use(System.Func<Microsoft.AspNetCore.Http.RequestDelegate, Microsoft.AspNetCore.Http.RequestDelegate> middleware);
    }
    public static partial class MapExtensions
    {
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder Map(this Microsoft.AspNetCore.Builder.IApplicationBuilder app, Microsoft.AspNetCore.Http.PathString pathMatch, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> configuration) { throw null; }
    }
    public static partial class MapWhenExtensions
    {
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder MapWhen(this Microsoft.AspNetCore.Builder.IApplicationBuilder app, System.Func<Microsoft.AspNetCore.Http.HttpContext, bool> predicate, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> configuration) { throw null; }
    }
    public static partial class RunExtensions
    {
        public static void Run(this Microsoft.AspNetCore.Builder.IApplicationBuilder app, Microsoft.AspNetCore.Http.RequestDelegate handler) { }
    }
    public static partial class UseExtensions
    {
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder Use(this Microsoft.AspNetCore.Builder.IApplicationBuilder app, System.Func<Microsoft.AspNetCore.Http.HttpContext, System.Func<System.Threading.Tasks.Task>, System.Threading.Tasks.Task> middleware) { throw null; }
    }
    public static partial class UseMiddlewareExtensions
    {
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseMiddleware(this Microsoft.AspNetCore.Builder.IApplicationBuilder app, System.Type middleware, params object[] args) { throw null; }
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseMiddleware<TMiddleware>(this Microsoft.AspNetCore.Builder.IApplicationBuilder app, params object[] args) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Builder.Extensions
{
    public partial class MapMiddleware
    {
        public MapMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, Microsoft.AspNetCore.Builder.Extensions.MapOptions options) { }
        public System.Threading.Tasks.Task Invoke(Microsoft.AspNetCore.Http.HttpContext context) { throw null; }
    }
    public partial class MapOptions
    {
        public MapOptions() { }
        public Microsoft.AspNetCore.Http.RequestDelegate Branch { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Http.PathString PathMatch { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class MapWhenMiddleware
    {
        public MapWhenMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, Microsoft.AspNetCore.Builder.Extensions.MapWhenOptions options) { }
        public System.Threading.Tasks.Task Invoke(Microsoft.AspNetCore.Http.HttpContext context) { throw null; }
    }
    public partial class MapWhenOptions
    {
        public MapWhenOptions() { }
        public Microsoft.AspNetCore.Http.RequestDelegate Branch { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Func<Microsoft.AspNetCore.Http.HttpContext, bool> Predicate { get { throw null; } set { } }
    }
}
namespace Microsoft.AspNetCore.Http
{
    public abstract partial class ConnectionInfo
    {
        protected ConnectionInfo() { }
        public abstract System.Security.Cryptography.X509Certificates.X509Certificate2 ClientCertificate { get; set; }
        public abstract System.Net.IPAddress LocalIpAddress { get; set; }
        public abstract int LocalPort { get; set; }
        public abstract System.Net.IPAddress RemoteIpAddress { get; set; }
        public abstract int RemotePort { get; set; }
        public abstract System.Threading.Tasks.Task<System.Security.Cryptography.X509Certificates.X509Certificate2> GetClientCertificateAsync(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
    }
    public enum CookieSecurePolicy
    {
        Always = 1,
        None = 2,
        SameAsRequest = 0,
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct FragmentString : System.IEquatable<Microsoft.AspNetCore.Http.FragmentString>
    {
        public static readonly Microsoft.AspNetCore.Http.FragmentString Empty;
        public FragmentString(string value) { throw null;}
        public bool HasValue { get { throw null; } }
        public string Value { get { throw null; } }
        public bool Equals(Microsoft.AspNetCore.Http.FragmentString other) { throw null; }
        public override bool Equals(object obj) { throw null; }
        public static Microsoft.AspNetCore.Http.FragmentString FromUriComponent(string uriComponent) { throw null; }
        public static Microsoft.AspNetCore.Http.FragmentString FromUriComponent(System.Uri uri) { throw null; }
        public override int GetHashCode() { throw null; }
        public static bool operator ==(Microsoft.AspNetCore.Http.FragmentString left, Microsoft.AspNetCore.Http.FragmentString right) { throw null; }
        public static bool operator !=(Microsoft.AspNetCore.Http.FragmentString left, Microsoft.AspNetCore.Http.FragmentString right) { throw null; }
        public override string ToString() { throw null; }
        public string ToUriComponent() { throw null; }
    }
    public static partial class HeaderDictionaryExtensions
    {
        public static void Append(this Microsoft.AspNetCore.Http.IHeaderDictionary headers, string key, Microsoft.Extensions.Primitives.StringValues value) { }
        public static void AppendCommaSeparatedValues(this Microsoft.AspNetCore.Http.IHeaderDictionary headers, string key, params string[] values) { }
        public static string[] GetCommaSeparatedValues(this Microsoft.AspNetCore.Http.IHeaderDictionary headers, string key) { throw null; }
        public static void SetCommaSeparatedValues(this Microsoft.AspNetCore.Http.IHeaderDictionary headers, string key, params string[] values) { }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct HostString : System.IEquatable<Microsoft.AspNetCore.Http.HostString>
    {
        public HostString(string value) { throw null;}
        public HostString(string host, int port) { throw null;}
        public bool HasValue { get { throw null; } }
        public string Host { get { throw null; } }
        public System.Nullable<int> Port { get { throw null; } }
        public string Value { get { throw null; } }
        public bool Equals(Microsoft.AspNetCore.Http.HostString other) { throw null; }
        public override bool Equals(object obj) { throw null; }
        public static Microsoft.AspNetCore.Http.HostString FromUriComponent(string uriComponent) { throw null; }
        public static Microsoft.AspNetCore.Http.HostString FromUriComponent(System.Uri uri) { throw null; }
        public override int GetHashCode() { throw null; }
        public static bool operator ==(Microsoft.AspNetCore.Http.HostString left, Microsoft.AspNetCore.Http.HostString right) { throw null; }
        public static bool operator !=(Microsoft.AspNetCore.Http.HostString left, Microsoft.AspNetCore.Http.HostString right) { throw null; }
        public override string ToString() { throw null; }
        public string ToUriComponent() { throw null; }
    }
    public abstract partial class HttpContext
    {
        protected HttpContext() { }
        public abstract Microsoft.AspNetCore.Http.Authentication.AuthenticationManager Authentication { get; }
        public abstract Microsoft.AspNetCore.Http.ConnectionInfo Connection { get; }
        public abstract Microsoft.AspNetCore.Http.Features.IFeatureCollection Features { get; }
        public abstract System.Collections.Generic.IDictionary<object, object> Items { get; set; }
        public abstract Microsoft.AspNetCore.Http.HttpRequest Request { get; }
        public abstract System.Threading.CancellationToken RequestAborted { get; set; }
        public abstract System.IServiceProvider RequestServices { get; set; }
        public abstract Microsoft.AspNetCore.Http.HttpResponse Response { get; }
        public abstract Microsoft.AspNetCore.Http.ISession Session { get; set; }
        public abstract string TraceIdentifier { get; set; }
        public abstract System.Security.Claims.ClaimsPrincipal User { get; set; }
        public abstract Microsoft.AspNetCore.Http.WebSocketManager WebSockets { get; }
        public abstract void Abort();
    }
    public abstract partial class HttpRequest
    {
        protected HttpRequest() { }
        public abstract System.IO.Stream Body { get; set; }
        public abstract System.Nullable<long> ContentLength { get; set; }
        public abstract string ContentType { get; set; }
        public abstract Microsoft.AspNetCore.Http.IRequestCookieCollection Cookies { get; set; }
        public abstract Microsoft.AspNetCore.Http.IFormCollection Form { get; set; }
        public abstract bool HasFormContentType { get; }
        public abstract Microsoft.AspNetCore.Http.IHeaderDictionary Headers { get; }
        public abstract Microsoft.AspNetCore.Http.HostString Host { get; set; }
        public abstract Microsoft.AspNetCore.Http.HttpContext HttpContext { get; }
        public abstract bool IsHttps { get; set; }
        public abstract string Method { get; set; }
        public abstract Microsoft.AspNetCore.Http.PathString Path { get; set; }
        public abstract Microsoft.AspNetCore.Http.PathString PathBase { get; set; }
        public abstract string Protocol { get; set; }
        public abstract Microsoft.AspNetCore.Http.IQueryCollection Query { get; set; }
        public abstract Microsoft.AspNetCore.Http.QueryString QueryString { get; set; }
        public abstract string Scheme { get; set; }
        public abstract System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IFormCollection> ReadFormAsync(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
    }
    public abstract partial class HttpResponse
    {
        protected HttpResponse() { }
        public abstract System.IO.Stream Body { get; set; }
        public abstract System.Nullable<long> ContentLength { get; set; }
        public abstract string ContentType { get; set; }
        public abstract Microsoft.AspNetCore.Http.IResponseCookies Cookies { get; }
        public abstract bool HasStarted { get; }
        public abstract Microsoft.AspNetCore.Http.IHeaderDictionary Headers { get; }
        public abstract Microsoft.AspNetCore.Http.HttpContext HttpContext { get; }
        public abstract int StatusCode { get; set; }
        public abstract void OnCompleted(System.Func<object, System.Threading.Tasks.Task> callback, object state);
        public virtual void OnCompleted(System.Func<System.Threading.Tasks.Task> callback) { }
        public abstract void OnStarting(System.Func<object, System.Threading.Tasks.Task> callback, object state);
        public virtual void OnStarting(System.Func<System.Threading.Tasks.Task> callback) { }
        public virtual void Redirect(string location) { }
        public abstract void Redirect(string location, bool permanent);
        public virtual void RegisterForDispose(System.IDisposable disposable) { }
    }
    public static partial class HttpResponseWritingExtensions
    {
        public static System.Threading.Tasks.Task WriteAsync(this Microsoft.AspNetCore.Http.HttpResponse response, string text, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken)) { throw null; }
        public static System.Threading.Tasks.Task WriteAsync(this Microsoft.AspNetCore.Http.HttpResponse response, string text, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken)) { throw null; }
    }
    public partial interface IHttpContextAccessor
    {
        Microsoft.AspNetCore.Http.HttpContext HttpContext { get; set; }
    }
    public partial interface IHttpContextFactory
    {
        Microsoft.AspNetCore.Http.HttpContext Create(Microsoft.AspNetCore.Http.Features.IFeatureCollection featureCollection);
        void Dispose(Microsoft.AspNetCore.Http.HttpContext httpContext);
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct PathString : System.IEquatable<Microsoft.AspNetCore.Http.PathString>
    {
        public static readonly Microsoft.AspNetCore.Http.PathString Empty;
        public PathString(string value) { throw null;}
        public bool HasValue { get { throw null; } }
        public string Value { get { throw null; } }
        public Microsoft.AspNetCore.Http.PathString Add(Microsoft.AspNetCore.Http.PathString other) { throw null; }
        public string Add(Microsoft.AspNetCore.Http.QueryString other) { throw null; }
        public bool Equals(Microsoft.AspNetCore.Http.PathString other) { throw null; }
        public bool Equals(Microsoft.AspNetCore.Http.PathString other, System.StringComparison comparisonType) { throw null; }
        public override bool Equals(object obj) { throw null; }
        public static Microsoft.AspNetCore.Http.PathString FromUriComponent(string uriComponent) { throw null; }
        public static Microsoft.AspNetCore.Http.PathString FromUriComponent(System.Uri uri) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.AspNetCore.Http.PathString operator +(Microsoft.AspNetCore.Http.PathString left, Microsoft.AspNetCore.Http.PathString right) { throw null; }
        public static string operator +(Microsoft.AspNetCore.Http.PathString left, Microsoft.AspNetCore.Http.QueryString right) { throw null; }
        public static string operator +(Microsoft.AspNetCore.Http.PathString left, string right) { throw null; }
        public static string operator +(string left, Microsoft.AspNetCore.Http.PathString right) { throw null; }
        public static bool operator ==(Microsoft.AspNetCore.Http.PathString left, Microsoft.AspNetCore.Http.PathString right) { throw null; }
        public static implicit operator string (Microsoft.AspNetCore.Http.PathString path) { throw null; }
        public static implicit operator Microsoft.AspNetCore.Http.PathString (string s) { throw null; }
        public static bool operator !=(Microsoft.AspNetCore.Http.PathString left, Microsoft.AspNetCore.Http.PathString right) { throw null; }
        public bool StartsWithSegments(Microsoft.AspNetCore.Http.PathString other) { throw null; }
        public bool StartsWithSegments(Microsoft.AspNetCore.Http.PathString other, out Microsoft.AspNetCore.Http.PathString remaining) { remaining = default(Microsoft.AspNetCore.Http.PathString); throw null; }
        public bool StartsWithSegments(Microsoft.AspNetCore.Http.PathString other, System.StringComparison comparisonType) { throw null; }
        public bool StartsWithSegments(Microsoft.AspNetCore.Http.PathString other, System.StringComparison comparisonType, out Microsoft.AspNetCore.Http.PathString remaining) { remaining = default(Microsoft.AspNetCore.Http.PathString); throw null; }
        public override string ToString() { throw null; }
        public string ToUriComponent() { throw null; }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct QueryString : System.IEquatable<Microsoft.AspNetCore.Http.QueryString>
    {
        public static readonly Microsoft.AspNetCore.Http.QueryString Empty;
        public QueryString(string value) { throw null;}
        public bool HasValue { get { throw null; } }
        public string Value { get { throw null; } }
        public Microsoft.AspNetCore.Http.QueryString Add(Microsoft.AspNetCore.Http.QueryString other) { throw null; }
        public Microsoft.AspNetCore.Http.QueryString Add(string name, string value) { throw null; }
        public static Microsoft.AspNetCore.Http.QueryString Create(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> parameters) { throw null; }
        public static Microsoft.AspNetCore.Http.QueryString Create(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> parameters) { throw null; }
        public static Microsoft.AspNetCore.Http.QueryString Create(string name, string value) { throw null; }
        public bool Equals(Microsoft.AspNetCore.Http.QueryString other) { throw null; }
        public override bool Equals(object obj) { throw null; }
        public static Microsoft.AspNetCore.Http.QueryString FromUriComponent(string uriComponent) { throw null; }
        public static Microsoft.AspNetCore.Http.QueryString FromUriComponent(System.Uri uri) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.AspNetCore.Http.QueryString operator +(Microsoft.AspNetCore.Http.QueryString left, Microsoft.AspNetCore.Http.QueryString right) { throw null; }
        public static bool operator ==(Microsoft.AspNetCore.Http.QueryString left, Microsoft.AspNetCore.Http.QueryString right) { throw null; }
        public static bool operator !=(Microsoft.AspNetCore.Http.QueryString left, Microsoft.AspNetCore.Http.QueryString right) { throw null; }
        public override string ToString() { throw null; }
        public string ToUriComponent() { throw null; }
    }
    public delegate System.Threading.Tasks.Task RequestDelegate(Microsoft.AspNetCore.Http.HttpContext context);
    public static partial class StatusCodes
    {
        public const int Status200OK = 200;
        public const int Status201Created = 201;
        public const int Status202Accepted = 202;
        public const int Status203NonAuthoritative = 203;
        public const int Status204NoContent = 204;
        public const int Status205ResetContent = 205;
        public const int Status206PartialContent = 206;
        public const int Status300MultipleChoices = 300;
        public const int Status301MovedPermanently = 301;
        public const int Status302Found = 302;
        public const int Status303SeeOther = 303;
        public const int Status304NotModified = 304;
        public const int Status305UseProxy = 305;
        public const int Status306SwitchProxy = 306;
        public const int Status307TemporaryRedirect = 307;
        public const int Status400BadRequest = 400;
        public const int Status401Unauthorized = 401;
        public const int Status402PaymentRequired = 402;
        public const int Status403Forbidden = 403;
        public const int Status404NotFound = 404;
        public const int Status405MethodNotAllowed = 405;
        public const int Status406NotAcceptable = 406;
        public const int Status407ProxyAuthenticationRequired = 407;
        public const int Status408RequestTimeout = 408;
        public const int Status409Conflict = 409;
        public const int Status410Gone = 410;
        public const int Status411LengthRequired = 411;
        public const int Status412PreconditionFailed = 412;
        public const int Status413RequestEntityTooLarge = 413;
        public const int Status414RequestUriTooLong = 414;
        public const int Status415UnsupportedMediaType = 415;
        public const int Status416RequestedRangeNotSatisfiable = 416;
        public const int Status417ExpectationFailed = 417;
        public const int Status418ImATeapot = 418;
        public const int Status419AuthenticationTimeout = 419;
        public const int Status500InternalServerError = 500;
        public const int Status501NotImplemented = 501;
        public const int Status502BadGateway = 502;
        public const int Status503ServiceUnavailable = 503;
        public const int Status504GatewayTimeout = 504;
        public const int Status505HttpVersionNotsupported = 505;
        public const int Status506VariantAlsoNegotiates = 506;
    }
    public abstract partial class WebSocketManager
    {
        protected WebSocketManager() { }
        public abstract bool IsWebSocketRequest { get; }
        public abstract System.Collections.Generic.IList<string> WebSocketRequestedProtocols { get; }
        public virtual System.Threading.Tasks.Task<System.Net.WebSockets.WebSocket> AcceptWebSocketAsync() { throw null; }
        public abstract System.Threading.Tasks.Task<System.Net.WebSockets.WebSocket> AcceptWebSocketAsync(string subProtocol);
    }
}
namespace Microsoft.AspNetCore.Http.Authentication
{
    public partial class AuthenticateInfo
    {
        public AuthenticateInfo() { }
        public Microsoft.AspNetCore.Http.Authentication.AuthenticationDescription Description { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Security.Claims.ClaimsPrincipal Principal { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties Properties { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class AuthenticationDescription
    {
        public AuthenticationDescription() { }
        public AuthenticationDescription(System.Collections.Generic.IDictionary<string, object> items) { }
        public string AuthenticationScheme { get { throw null; } set { } }
        public string DisplayName { get { throw null; } set { } }
        public System.Collections.Generic.IDictionary<string, object> Items { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public abstract partial class AuthenticationManager
    {
        public const string AutomaticScheme = "Automatic";
        protected AuthenticationManager() { }
        public abstract Microsoft.AspNetCore.Http.HttpContext HttpContext { get; }
        public abstract System.Threading.Tasks.Task AuthenticateAsync(Microsoft.AspNetCore.Http.Features.Authentication.AuthenticateContext context);
        public virtual System.Threading.Tasks.Task<System.Security.Claims.ClaimsPrincipal> AuthenticateAsync(string authenticationScheme) { throw null; }
        public virtual System.Threading.Tasks.Task ChallengeAsync() { throw null; }
        public virtual System.Threading.Tasks.Task ChallengeAsync(Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties properties) { throw null; }
        public virtual System.Threading.Tasks.Task ChallengeAsync(string authenticationScheme) { throw null; }
        public virtual System.Threading.Tasks.Task ChallengeAsync(string authenticationScheme, Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties properties) { throw null; }
        public abstract System.Threading.Tasks.Task ChallengeAsync(string authenticationScheme, Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties properties, Microsoft.AspNetCore.Http.Features.Authentication.ChallengeBehavior behavior);
        public virtual System.Threading.Tasks.Task ForbidAsync() { throw null; }
        public virtual System.Threading.Tasks.Task ForbidAsync(Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties properties) { throw null; }
        public virtual System.Threading.Tasks.Task ForbidAsync(string authenticationScheme) { throw null; }
        public virtual System.Threading.Tasks.Task ForbidAsync(string authenticationScheme, Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties properties) { throw null; }
        public abstract System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.Authentication.AuthenticateInfo> GetAuthenticateInfoAsync(string authenticationScheme);
        public abstract System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Http.Authentication.AuthenticationDescription> GetAuthenticationSchemes();
        public virtual System.Threading.Tasks.Task SignInAsync(string authenticationScheme, System.Security.Claims.ClaimsPrincipal principal) { throw null; }
        public abstract System.Threading.Tasks.Task SignInAsync(string authenticationScheme, System.Security.Claims.ClaimsPrincipal principal, Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties properties);
        public virtual System.Threading.Tasks.Task SignOutAsync(string authenticationScheme) { throw null; }
        public abstract System.Threading.Tasks.Task SignOutAsync(string authenticationScheme, Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties properties);
    }
    public partial class AuthenticationProperties
    {
        public AuthenticationProperties() { }
        public AuthenticationProperties(System.Collections.Generic.IDictionary<string, string> items) { }
        public System.Nullable<bool> AllowRefresh { get { throw null; } set { } }
        public System.Nullable<System.DateTimeOffset> ExpiresUtc { get { throw null; } set { } }
        public bool IsPersistent { get { throw null; } set { } }
        public System.Nullable<System.DateTimeOffset> IssuedUtc { get { throw null; } set { } }
        public System.Collections.Generic.IDictionary<string, string> Items { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string RedirectUri { get { throw null; } set { } }
    }
}
