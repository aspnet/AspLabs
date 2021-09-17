$repoRoot = Resolve-Path $PSScriptRoot/../..;
Remove-Item -Recurse -Force $repoRoot/artifacts/obj/Microsoft.AspNetCore.Components.WebAssembly.Packaging/, $repoRoot/bin/Microsoft.AspNetCore.Components.WebAssembly.Packaging/ -ErrorAction SilentlyContinue;
Push-Location $repoRoot/src/BlazorWebAssemblyCustomInitialization/Microsoft.AspNetCore.Components.WebAssembly.Packaging;
dotnet pack;
Remove-Item -Recurse -Force $env:USERPROFILE/.nuget/packages/Microsoft.AspNetCore.Components.WebAssembly.Packaging -ErrorAction SilentlyContinue;
Copy-Item $repoRoot/artifacts/packages/Debug/Shipping/Microsoft.AspNetCore.Components.WebAssembly.Packaging.0.1.0.nupkg -Destination $repoRoot/src/BlazorWebAssemblyCustomInitialization/sample/.local/
Pop-Location;
Push-Location $repoRoot/src/BlazorWebAssemblyCustomInitialization/sample/CustomPackagedApp/Server;
dotnet publish /bl;
Push-Location $repoRoot/artifacts/bin/CustomPackagedApp.Server/Debug/net6.0/publish/; ./CustomPackagedApp.Server.exe
