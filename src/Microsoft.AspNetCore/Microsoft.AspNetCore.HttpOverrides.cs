namespace Microsoft.AspNetCore.Builder
{
    public static partial class ForwardedHeadersExtensions
    {
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseForwardedHeaders(this Microsoft.AspNetCore.Builder.IApplicationBuilder builder) { throw null; }
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseForwardedHeaders(this Microsoft.AspNetCore.Builder.IApplicationBuilder builder, Microsoft.AspNetCore.Builder.ForwardedHeadersOptions options) { throw null; }
    }
    public partial class ForwardedHeadersOptions
    {
        public ForwardedHeadersOptions() { }
        public Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders ForwardedHeaders { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Nullable<int> ForwardLimit { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.HttpOverrides.IPNetwork> KnownNetworks { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IList<System.Net.IPAddress> KnownProxies { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool RequireHeaderSymmetry { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public static partial class HttpMethodOverrideExtensions
    {
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseHttpMethodOverride(this Microsoft.AspNetCore.Builder.IApplicationBuilder builder) { throw null; }
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseHttpMethodOverride(this Microsoft.AspNetCore.Builder.IApplicationBuilder builder, Microsoft.AspNetCore.Builder.HttpMethodOverrideOptions options) { throw null; }
    }
    public partial class HttpMethodOverrideOptions
    {
        public HttpMethodOverrideOptions() { }
        public string FormFieldName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
}
namespace Microsoft.AspNetCore.HttpOverrides
{
    [System.FlagsAttribute]
    public enum ForwardedHeaders
    {
        All = 7,
        None = 0,
        XForwardedFor = 1,
        XForwardedHost = 2,
        XForwardedProto = 4,
    }
    public partial class ForwardedHeadersMiddleware
    {
        public ForwardedHeadersMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions> options) { }
        public void ApplyForwarders(Microsoft.AspNetCore.Http.HttpContext context) { }
        public System.Threading.Tasks.Task Invoke(Microsoft.AspNetCore.Http.HttpContext context) { throw null; }
    }
    public partial class HttpMethodOverrideMiddleware
    {
        public HttpMethodOverrideMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Builder.HttpMethodOverrideOptions> options) { }
        public System.Threading.Tasks.Task Invoke(Microsoft.AspNetCore.Http.HttpContext context) { throw null; }
    }
    public partial class IPNetwork
    {
        public IPNetwork(System.Net.IPAddress prefix, int prefixLength) { }
        public System.Net.IPAddress Prefix { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public int PrefixLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Contains(System.Net.IPAddress address) { throw null; }
    }
}
namespace Microsoft.AspNetCore.HttpOverrides.Internal
{
    public static partial class IPEndPointParser
    {
        public static bool TryParse(string addressWithPort, out System.Net.IPEndPoint endpoint) { endpoint = default(System.Net.IPEndPoint); throw null; }
    }
}
