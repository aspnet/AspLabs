## Blazor custom packaging sample

This sample demonstrates how to transform the publish output of a blazor webassembly application via a package to support scenarios where the default loading process causes issues on a given environment.

The sample is composed of 3 projects that deal with building and serving the custom boot extension and a sample app that demonstrates how the packages are consumed.

Microsoft.AspNetCore.Components.WebAssembly.Packaging contains MSBuild targets to customize the Blazor publish output as well as a JS initializer for handling the loading of resources for the custom loading process.

Microsoft.AspNetCore.Components.WebAssembly.Packaging.Tasks contains an MSBuild task that is consumed by Microsoft.AspNetCore.Components.WebAssembly.Packaging MSBuild targets and that is responsible for generating the contents of the bundle.

Microsoft.AspNetCore.Components.WebAssembly.Packaging.Server contains a helper method to serve the custom bundle contents as well as the compressed versions of the bundle.

## Building the sample and running the sample app
* Run build.cmd at the root of the repository.
* Copy the packages `Microsoft.AspNetCore.Components.WebAssembly.Packaging` and `Microsoft.AspNetCore.Components.WebAssembly.Packaging.Server` from `$(RepoRoot)artifacts\packages\Debug\Shipping` to `./sample/.local`
* Navigate to ./samples/Server and run dotnet publish
* Navigate to the publish folder and run CustomPackagedApp.Server.exe
* Open the browser and navigate to https://localhost:5001

## Tailoring the publish output for your own environment.
* Copy the code from `src\BlazorWebAssemblyCustomInitialization\Microsoft.AspNetCore.Components.WebAssembly.*` into your own repository.
* Rename projects and namespaces to suit your needs.
* Update the task in Microsoft.AspNetCore.Components.WebAssembly.Packaging.Tasks to perform whatever transformation you want on the blazor publish assets.
* Update the JS initializer in Microsoft.AspNetCore.Components.WebAssembly.Packaging as well as the MSBuild targets if necessary.
* Update the helper extension on the server to ensure the files are correctly served.
