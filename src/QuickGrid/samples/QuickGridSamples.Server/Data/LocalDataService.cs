using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using QuickGridSamples.Core.Models;

namespace QuickGridSamples.Server.Data;

public class LocalDataService : IDataService
{
    private readonly ApplicationDbContext _dbContext;

    public LocalDataService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Country> Countries => _dbContext.Countries;

    public async Task<GridItemsProviderResult<Country>> GetCountriesAsync(int startIndex, int? count, string sortBy, bool sortAscending, CancellationToken cancellationToken)
    {
        var ordered = (sortBy, sortAscending) switch
        {
            (nameof(Country.Name), true) => _dbContext.Countries.OrderBy(c => c.Name),
            (nameof(Country.Name), false) => _dbContext.Countries.OrderByDescending(c => c.Name),
            (nameof(Country.Code), true) => _dbContext.Countries.OrderBy(c => c.Code),
            (nameof(Country.Code), false) => _dbContext.Countries.OrderByDescending(c => c.Code),
            ("Medals.Gold", true) => _dbContext.Countries.OrderBy(c => c.Medals.Gold),
            ("Medals.Gold", false) => _dbContext.Countries.OrderByDescending(c => c.Medals.Gold),
            _ => _dbContext.Countries.OrderByDescending(c => c.Medals.Gold),
        };

        var result = ordered.Skip(startIndex);

        if (count.HasValue)
        {
            result = result.Take(count.Value);
        }

        return GridItemsProviderResult.From(await result.ToListAsync(cancellationToken), await ordered.CountAsync());
    }
}
