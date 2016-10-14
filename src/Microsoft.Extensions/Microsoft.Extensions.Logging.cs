namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class LoggingServiceCollectionExtensions
    {
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddLogging(this Microsoft.Extensions.DependencyInjection.IServiceCollection services) { throw null; }
    }
}
namespace Microsoft.Extensions.Logging
{
    public partial class LoggerFactory : Microsoft.Extensions.Logging.ILoggerFactory, System.IDisposable
    {
        public LoggerFactory() { }
        public void AddProvider(Microsoft.Extensions.Logging.ILoggerProvider provider) { }
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) { throw null; }
        public void Dispose() { }
    }
}
