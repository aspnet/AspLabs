namespace Microsoft.AspNetCore.Hosting
{
    public static partial class EnvironmentName
    {
        public static readonly string Development;
        public static readonly string Production;
        public static readonly string Staging;
    }
    public static partial class HostingAbstractionsWebHostBuilderExtensions
    {
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder CaptureStartupErrors(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, bool captureStartupErrors) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHost Start(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, params string[] urls) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseConfiguration(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, Microsoft.Extensions.Configuration.IConfiguration configuration) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseContentRoot(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, string contentRoot) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseEnvironment(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, string environment) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseServer(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, Microsoft.AspNetCore.Hosting.Server.IServer server) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseStartup(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, string startupAssemblyName) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseUrls(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, params string[] urls) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseWebRoot(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, string webRoot) { throw null; }
    }
    public static partial class HostingEnvironmentExtensions
    {
        public static bool IsDevelopment(this Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment) { throw null; }
        public static bool IsEnvironment(this Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, string environmentName) { throw null; }
        public static bool IsProduction(this Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment) { throw null; }
        public static bool IsStaging(this Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment) { throw null; }
    }
    public partial interface IApplicationLifetime
    {
        System.Threading.CancellationToken ApplicationStarted { get; }
        System.Threading.CancellationToken ApplicationStopped { get; }
        System.Threading.CancellationToken ApplicationStopping { get; }
        void StopApplication();
    }
    public partial interface IHostingEnvironment
    {
        string ApplicationName { get; set; }
        Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
        string ContentRootPath { get; set; }
        string EnvironmentName { get; set; }
        Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
        string WebRootPath { get; set; }
    }
    public partial interface IStartup
    {
        void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder app);
        System.IServiceProvider ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services);
    }
    public partial interface IStartupFilter
    {
        System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next);
    }
    public partial interface IWebHost : System.IDisposable
    {
        Microsoft.AspNetCore.Http.Features.IFeatureCollection ServerFeatures { get; }
        System.IServiceProvider Services { get; }
        void Start();
    }
    public partial interface IWebHostBuilder
    {
        Microsoft.AspNetCore.Hosting.IWebHost Build();
        Microsoft.AspNetCore.Hosting.IWebHostBuilder ConfigureLogging(System.Action<Microsoft.Extensions.Logging.ILoggerFactory> configureLogging);
        Microsoft.AspNetCore.Hosting.IWebHostBuilder ConfigureServices(System.Action<Microsoft.Extensions.DependencyInjection.IServiceCollection> configureServices);
        string GetSetting(string key);
        Microsoft.AspNetCore.Hosting.IWebHostBuilder UseLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory);
        Microsoft.AspNetCore.Hosting.IWebHostBuilder UseSetting(string key, string value);
    }
    public static partial class WebHostDefaults
    {
        public static readonly string ApplicationKey;
        public static readonly string CaptureStartupErrorsKey;
        public static readonly string ContentRootKey;
        public static readonly string DetailedErrorsKey;
        public static readonly string EnvironmentKey;
        public static readonly string ServerUrlsKey;
        public static readonly string StartupAssemblyKey;
        public static readonly string WebRootKey;
    }
}
