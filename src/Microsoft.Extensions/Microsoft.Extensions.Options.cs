namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OptionsServiceCollectionExtensions
    {
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddOptions(this Microsoft.Extensions.DependencyInjection.IServiceCollection services) { throw null; }
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection Configure<TOptions>(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, System.Action<TOptions> configureOptions) where TOptions : class { throw null; }
    }
}
namespace Microsoft.Extensions.Options
{
    public partial class ConfigureOptions<TOptions> : Microsoft.Extensions.Options.IConfigureOptions<TOptions> where TOptions : class
    {
        public ConfigureOptions(System.Action<TOptions> action) { }
        public System.Action<TOptions> Action { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public virtual void Configure(TOptions options) { }
    }
    public partial interface IConfigureOptions<in TOptions> where TOptions : class
    {
        void Configure(TOptions options);
    }
    public partial interface IOptions<out TOptions> where TOptions : class, new()
    {
        TOptions Value { get; }
    }
    public partial interface IOptionsMonitor<out TOptions>
    {
        TOptions CurrentValue { get; }
        System.IDisposable OnChange(System.Action<TOptions> listener);
    }
    public static partial class Options
    {
        public static Microsoft.Extensions.Options.IOptions<TOptions> Create<TOptions>(TOptions options) where TOptions : class, new() { throw null; }
    }
    public partial class OptionsManager<TOptions> : Microsoft.Extensions.Options.IOptions<TOptions> where TOptions : class, new()
    {
        public OptionsManager(System.Collections.Generic.IEnumerable<Microsoft.Extensions.Options.IConfigureOptions<TOptions>> setups) { }
        public virtual TOptions Value { get { throw null; } }
    }
    public partial class OptionsWrapper<TOptions> : Microsoft.Extensions.Options.IOptions<TOptions> where TOptions : class, new()
    {
        public OptionsWrapper(TOptions options) { }
        public TOptions Value { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
}
