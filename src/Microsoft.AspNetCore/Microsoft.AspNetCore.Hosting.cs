namespace Microsoft.AspNetCore.Hosting
{
    public partial class ConventionBasedStartup : Microsoft.AspNetCore.Hosting.IStartup
    {
        public ConventionBasedStartup(Microsoft.AspNetCore.Hosting.Internal.StartupMethods methods) { }
        public void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder app) { }
        public System.IServiceProvider ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { throw null; }
    }
    public partial class DelegateStartup : Microsoft.AspNetCore.Hosting.StartupBase
    {
        public DelegateStartup(System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> configureApp) { }
        public override void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder app) { }
    }
    public abstract partial class StartupBase : Microsoft.AspNetCore.Hosting.IStartup
    {
        protected StartupBase() { }
        public abstract void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder app);
        public virtual System.IServiceProvider ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { throw null; }
    }
    public partial class WebHostBuilder : Microsoft.AspNetCore.Hosting.IWebHostBuilder
    {
        public WebHostBuilder() { }
        public Microsoft.AspNetCore.Hosting.IWebHost Build() { throw null; }
        public Microsoft.AspNetCore.Hosting.IWebHostBuilder ConfigureLogging(System.Action<Microsoft.Extensions.Logging.ILoggerFactory> configureLogging) { throw null; }
        public Microsoft.AspNetCore.Hosting.IWebHostBuilder ConfigureServices(System.Action<Microsoft.Extensions.DependencyInjection.IServiceCollection> configureServices) { throw null; }
        public string GetSetting(string key) { throw null; }
        public Microsoft.AspNetCore.Hosting.IWebHostBuilder UseLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory) { throw null; }
        public Microsoft.AspNetCore.Hosting.IWebHostBuilder UseSetting(string key, string value) { throw null; }
    }
    public static partial class WebHostBuilderExtensions
    {
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder Configure(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> configureApp) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseStartup(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, System.Type startupType) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseStartup<TStartup>(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder) where TStartup : class { throw null; }
    }
    public static partial class WebHostExtensions
    {
        public static void Run(this Microsoft.AspNetCore.Hosting.IWebHost host) { }
        public static void Run(this Microsoft.AspNetCore.Hosting.IWebHost host, System.Threading.CancellationToken token) { }
    }
}
namespace Microsoft.AspNetCore.Hosting.Builder
{
    public partial class ApplicationBuilderFactory : Microsoft.AspNetCore.Hosting.Builder.IApplicationBuilderFactory
    {
        public ApplicationBuilderFactory(System.IServiceProvider serviceProvider) { }
        public Microsoft.AspNetCore.Builder.IApplicationBuilder CreateBuilder(Microsoft.AspNetCore.Http.Features.IFeatureCollection serverFeatures) { throw null; }
    }
    public partial interface IApplicationBuilderFactory
    {
        Microsoft.AspNetCore.Builder.IApplicationBuilder CreateBuilder(Microsoft.AspNetCore.Http.Features.IFeatureCollection serverFeatures);
    }
}
namespace Microsoft.AspNetCore.Hosting.Internal
{
    public partial class ApplicationLifetime : Microsoft.AspNetCore.Hosting.IApplicationLifetime
    {
        public ApplicationLifetime() { }
        public System.Threading.CancellationToken ApplicationStarted { get { throw null; } }
        public System.Threading.CancellationToken ApplicationStopped { get { throw null; } }
        public System.Threading.CancellationToken ApplicationStopping { get { throw null; } }
        public void NotifyStarted() { }
        public void NotifyStopped() { }
        public void StopApplication() { }
    }
    public partial class AutoRequestServicesStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
    {
        public AutoRequestServicesStartupFilter() { }
        public System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next) { throw null; }
    }
    public partial class ConfigureBuilder
    {
        public ConfigureBuilder(System.Reflection.MethodInfo configure) { }
        public System.Reflection.MethodInfo MethodInfo { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Build(object instance) { throw null; }
    }
    public partial class ConfigureServicesBuilder
    {
        public ConfigureServicesBuilder(System.Reflection.MethodInfo configureServices) { }
        public System.Reflection.MethodInfo MethodInfo { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Func<Microsoft.Extensions.DependencyInjection.IServiceCollection, System.IServiceProvider> Build(object instance) { throw null; }
    }
    public partial class HostingApplication : Microsoft.AspNetCore.Hosting.Server.IHttpApplication<Microsoft.AspNetCore.Hosting.Internal.HostingApplication.Context>
    {
        public HostingApplication(Microsoft.AspNetCore.Http.RequestDelegate application, Microsoft.Extensions.Logging.ILogger logger, System.Diagnostics.DiagnosticSource diagnosticSource, Microsoft.AspNetCore.Http.IHttpContextFactory httpContextFactory) { }
        public Microsoft.AspNetCore.Hosting.Internal.HostingApplication.Context CreateContext(Microsoft.AspNetCore.Http.Features.IFeatureCollection contextFeatures) { throw null; }
        public void DisposeContext(Microsoft.AspNetCore.Hosting.Internal.HostingApplication.Context context, System.Exception exception) { }
        public System.Threading.Tasks.Task ProcessRequestAsync(Microsoft.AspNetCore.Hosting.Internal.HostingApplication.Context context) { throw null; }
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public partial struct Context
        {
            public Microsoft.AspNetCore.Http.HttpContext HttpContext { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
            public System.IDisposable Scope { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
            public long StartTimestamp { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        }
    }
    public partial class HostingEnvironment : Microsoft.AspNetCore.Hosting.IHostingEnvironment
    {
        public HostingEnvironment() { }
        public string ApplicationName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string ContentRootPath { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string EnvironmentName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string WebRootPath { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public static partial class HostingEnvironmentExtensions
    {
        public static void Initialize(this Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, string applicationName, string contentRootPath, Microsoft.AspNetCore.Hosting.Internal.WebHostOptions options) { }
    }
    public partial class RequestServicesContainerMiddleware
    {
        public RequestServicesContainerMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory) { }
        public System.Threading.Tasks.Task Invoke(Microsoft.AspNetCore.Http.HttpContext httpContext) { throw null; }
    }
    public partial class RequestServicesFeature : Microsoft.AspNetCore.Http.Features.IServiceProvidersFeature, System.IDisposable
    {
        public RequestServicesFeature(Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory) { }
        public System.IServiceProvider RequestServices { get { throw null; } set { } }
        public void Dispose() { }
    }
    public partial class StartupLoader
    {
        public StartupLoader() { }
        public static System.Type FindStartupType(string startupAssemblyName, string environmentName) { throw null; }
        public static Microsoft.AspNetCore.Hosting.Internal.StartupMethods LoadMethods(System.IServiceProvider services, System.Type startupType, string environmentName) { throw null; }
    }
    public partial class StartupMethods
    {
        public StartupMethods(System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> configure) { }
        public StartupMethods(System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> configure, System.Func<Microsoft.Extensions.DependencyInjection.IServiceCollection, System.IServiceProvider> configureServices) { }
        public System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> ConfigureDelegate { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Func<Microsoft.Extensions.DependencyInjection.IServiceCollection, System.IServiceProvider> ConfigureServicesDelegate { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public partial class WebHost : Microsoft.AspNetCore.Hosting.IWebHost, System.IDisposable
    {
        public WebHost(Microsoft.Extensions.DependencyInjection.IServiceCollection appServices, System.IServiceProvider hostingServiceProvider, Microsoft.AspNetCore.Hosting.Internal.WebHostOptions options, Microsoft.Extensions.Configuration.IConfiguration config) { }
        public Microsoft.AspNetCore.Http.Features.IFeatureCollection ServerFeatures { get { throw null; } }
        public System.IServiceProvider Services { get { throw null; } }
        public void Dispose() { }
        public void Initialize() { }
        public virtual void Start() { }
    }
    public partial class WebHostOptions
    {
        public WebHostOptions() { }
        public WebHostOptions(Microsoft.Extensions.Configuration.IConfiguration configuration) { }
        public string ApplicationName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool CaptureStartupErrors { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string ContentRootPath { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool DetailedErrors { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string Environment { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string StartupAssembly { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string WebRoot { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
}
namespace Microsoft.AspNetCore.Hosting.Server.Features
{
    public partial class ServerAddressesFeature : Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature
    {
        public ServerAddressesFeature() { }
        public System.Collections.Generic.ICollection<string> Addresses { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
}
