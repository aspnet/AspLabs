using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using QuickGridSamples.Core.Models;

namespace QuickGridSamples.WebAssembly;

internal class WebApiDataService : IDataService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;

    public WebApiDataService(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
    }

    public IQueryable<Country> Countries => throw new NotImplementedException();

    public async Task<GridItemsProviderResult<Country>> GetCountriesAsync(int startIndex, int? count, string sortBy, bool sortAscending, CancellationToken cancellationToken)
    {
        var url = _navigationManager.GetUriWithQueryParameters("/api/countries", new Dictionary<string, object>
        {
            { "startIndex", startIndex },
            { "count", count },
            { "sortBy", sortBy },
            { "sortAscending", sortAscending },
        });
        return await _httpClient.GetFromJsonAsync<GridItemsProviderResult<Country>>(url);
    }
}
