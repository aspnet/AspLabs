$ErrorActionPreference = 'stop'

mkdir -p "$PSScriptRoot/obj" -ea ignore | out-null

function Invoke-Block ([scriptblock] $_cmd) {
    & $_cmd
    if ((-not $?) -or ($LASTEXITCODE -ne 0)) {
        throw "command failed"
    }
}

$nuget = "$PSScriptRoot/obj/nuget.exe"
if (-not (Test-Path $nuget)) {
    iwr -o $nuget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
}

$dotnet = "$PSScriptRoot/.dotnet/dotnet"
$dotnetInstall = "$PSScriptRoot/obj/dotnet-install.ps1"
if (-not (Test-Path $dotnetInstall)) {
    iwr -o $dotnetInstall https://raw.githubusercontent.com/dotnet/cli/release/2.1.3xx/scripts/obtain/dotnet-install.ps1
}
& $dotnetInstall -installdir "$PSScriptRoot/.dotnet" -version 2.1.300
Copy-Item -Recurse "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App/*" "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly" -ea Ignore
mv "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly/2.1.0/Microsoft.AspNetCore.App.deps.json" "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly/2.1.0/Microsoft.AspNetCore.App.RefOnly.deps.json" -ea Ignore
mv "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly/2.1.0/Microsoft.AspNetCore.App.runtimeconfig.json" "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly/2.1.0/Microsoft.AspNetCore.App.RefOnly.runtimeconfig.json" -ea Ignore
$refsPublish = "$PSScriptRoot/Microsoft.AspNetCore.App.RefOnly/ref/netcoreapp2.1"
Invoke-Block { & $dotnet publish Microsoft.AspNetCore.App.RefOnly/Microsoft.AspNetCore.App.RefOnly.csproj --output $refsPublish }
rm "$refsPublish/Microsoft.AspNetCore.App.RefOnly.dll"
rm "$refsPublish/runtimes/" -Recurse
rm "$refsPublish/*.json"
Invoke-Block { & $nuget pack "$PSScriptRoot/Microsoft.AspNetCore.App.RefOnly/Microsoft.AspNetCore.App.RefOnly.nuspec" -OutputDirectory "$PSScriptRoot/feed" }

rm $env:USERPROFILE/.nuget/packages/Microsoft.AspNetCore.App.RefOnly -Recurse -ea ignore
Invoke-Block { & $dotnet publish test/test.csproj -o "$(pwd)/publish/" }

pushd publish
try {
    Invoke-Block { & $dotnet test.dll }
}
finally {
    popd
}
