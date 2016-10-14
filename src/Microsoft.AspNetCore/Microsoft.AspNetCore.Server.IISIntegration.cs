namespace Microsoft.AspNetCore.Builder
{
    public partial class IISOptions
    {
        public IISOptions() { }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.Http.Authentication.AuthenticationDescription> AuthenticationDescriptions { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool AutomaticAuthentication { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool ForwardClientCertificate { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool ForwardWindowsAuthentication { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
}
namespace Microsoft.AspNetCore.Hosting
{
    public static partial class WebHostBuilderIISExtensions
    {
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseIISIntegration(this Microsoft.AspNetCore.Hosting.IWebHostBuilder app) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Server.IISIntegration
{
    public partial class IISDefaults
    {
        public const string Negotiate = "Negotiate";
        public const string Ntlm = "NTLM";
        public IISDefaults() { }
    }
    public partial class IISMiddleware
    {
        public IISMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Builder.IISOptions> options, string pairingToken) { }
        public System.Threading.Tasks.Task Invoke(Microsoft.AspNetCore.Http.HttpContext httpContext) { throw null; }
    }
}
