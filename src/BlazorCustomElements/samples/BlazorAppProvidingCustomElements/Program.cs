using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorAppProvidingCustomElements;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.RegisterAsCustomElement<Counter>("my-blazor-counter");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
