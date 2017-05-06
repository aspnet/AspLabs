$ErrorActionPreference = 'Stop'

$artifacts = "$PSScriptRoot/artifacts"
if (Test-Path $artifacts) {
    Remove-Item -Recurse $artifacts
}

remove-item -Recurse $env:USERPROFILE/.nuget/packages/microsoft.aspnetcore.app/

dotnet restore src/Microsoft.AspNetCore.App/
dotnet pack src/Microsoft.AspNetCore.App/ -c Release -o $artifacts/build/
dotnet publish src/Microsoft.AspNetCore.App/ -r win7-x64 -c Release -o $artifacts/shared/Microsoft.AspNetCore.App/2.0.0-preview1/
