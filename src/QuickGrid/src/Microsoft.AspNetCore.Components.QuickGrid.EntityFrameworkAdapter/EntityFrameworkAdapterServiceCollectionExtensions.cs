using Microsoft.AspNetCore.Components.QuickGrid.EntityFrameworkAdapter;
using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to configure <see cref="IAsyncQueryExecutor"/> on a <see cref="IServiceCollection"/>.
/// </summary>
public static class EntityFrameworkAdapterServiceCollectionExtensions
{
    /// <summary>
    /// Registers an Entity Framework aware implementation of <see cref="IAsyncQueryExecutor"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public static void AddQuickGridEntityFrameworkAdapter(this IServiceCollection services)
    {
        services.AddSingleton<IAsyncQueryExecutor, EntityFrameworkAsyncQueryExecutor>();
    }
}
