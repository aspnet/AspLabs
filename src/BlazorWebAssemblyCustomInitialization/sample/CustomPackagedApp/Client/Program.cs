using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CustomPackagedApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CustomPackagedApp.Client.Pages.Index>("#app");

await builder.Build().RunAsync();
