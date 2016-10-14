namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OptionsConfigurationServiceCollectionExtensions
    {
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection Configure<TOptions>(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration config) where TOptions : class { throw null; }
    }
}
namespace Microsoft.Extensions.Options
{
    public partial class ConfigureFromConfigurationOptions<TOptions> : Microsoft.Extensions.Options.ConfigureOptions<TOptions> where TOptions : class
    {
        public ConfigureFromConfigurationOptions(Microsoft.Extensions.Configuration.IConfiguration config) : base (default(System.Action<TOptions>)) { }
    }
}
