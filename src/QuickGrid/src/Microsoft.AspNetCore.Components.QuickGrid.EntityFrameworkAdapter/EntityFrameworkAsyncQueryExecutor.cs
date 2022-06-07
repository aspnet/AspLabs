using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.AspNetCore.Components.QuickGrid.EntityFrameworkAdapter;

internal class EntityFrameworkAsyncQueryExecutor : IAsyncQueryExecutor
{
    public bool IsSupported<T>(IQueryable<T> queryable)
        => queryable.Provider is IAsyncQueryProvider;

    public Task<int> CountAsync<T>(IQueryable<T> queryable)
        => queryable.CountAsync();

    public Task<T[]> ToArrayAsync<T>(IQueryable<T> queryable)
        => queryable.ToArrayAsync();
}
