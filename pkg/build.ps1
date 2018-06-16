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

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1
$env:DOTNET_MULTILEVEL_LOOKUP = 0
$env:MSBUILDDISABLENODEREUSE = 1
$env:UseRazorBuildServer='false'

# Install a hackable copy of the 2.1.300 SDK. We're going to mess with it, so don't use the machine installed version
Write-Host ""
Write-Host -f Cyan "Setting up a local copy of the .NET Core 2.1.300 SDK that we're going to hack to pieces"
Write-Host ""
$dotnetInstall = "$PSScriptRoot/obj/dotnet-install.ps1"
if (-not (Test-Path $dotnetInstall)) {
    iwr -o $dotnetInstall https://raw.githubusercontent.com/dotnet/cli/release/2.1.3xx/scripts/obtain/dotnet-install.ps1
}
& $dotnetInstall -installdir "$PSScriptRoot/.dotnet" -version 2.1.300
$dotnet = Resolve-Path "$PSScriptRoot/.dotnet/dotnet.exe"

# Let's make a shared framework that is functionally the same as Microsoft.AspNetCore.App, but is referenced from a package that does not have nupkg dependencies
Write-Host ""
Write-Host -f Cyan "Creating the 'RefOnly' version of AspNetCore.App"
Write-Host ""
Copy-Item -Recurse "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App/*" "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly" -ea Ignore
mv "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly/2.1.0/Microsoft.AspNetCore.App.deps.json" "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly/2.1.0/Microsoft.AspNetCore.App.RefOnly.deps.json" -ea Ignore
mv "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly/2.1.0/Microsoft.AspNetCore.App.runtimeconfig.json" "$PSScriptRoot/.dotnet/shared/Microsoft.AspNetCore.App.RefOnly/2.1.0/Microsoft.AspNetCore.App.RefOnly.runtimeconfig.json" -ea Ignore
$refsPublish = "$PSScriptRoot/src/Microsoft.AspNetCore.App.RefOnly/ref/netcoreapp2.1"
Invoke-Block { & $dotnet publish src/Microsoft.AspNetCore.App.RefOnly/Microsoft.AspNetCore.App.RefOnly.csproj --output $refsPublish -nologo -v:q }
rm "$refsPublish/Microsoft.AspNetCore.App.RefOnly.dll"
rm "$refsPublish/Microsoft.AspNetCore.App.RefOnly.pdb"
rm "$refsPublish/runtimes/" -Recurse
rm "$refsPublish/*.json"
Remove-Item -Recurse -Force "$PSScriptRoot/obj/feed" -ea ignore
Invoke-Block { & $nuget pack "$PSScriptRoot/src/Microsoft.AspNetCore.App.RefOnly/Microsoft.AspNetCore.App.RefOnly.nuspec" -OutputDirectory "$PSScriptRoot/obj/feed" -NoPackageAnalysis }
rm $env:USERPROFILE/.nuget/packages/Microsoft.AspNetCore.App.RefOnly -Recurse -ea ignore

# Save a backup copy of the .lzma, just for comparison
$bigLzma = "$PSScriptRoot/.dotnet/sdk/2.1.300/nuGetPackagesArchive.lzma.bak"
$newLzma = "$PSScriptRoot/.dotnet/sdk/2.1.300/nuGetPackagesArchive.lzma"
if (-not (Test-Path $bigLzma)) {
    Copy-Item "$PSScriptRoot/.dotnet/sdk/2.1.300/nuGetPackagesArchive.lzma" $bigLzma
}
Remove-Item "$PSScriptRoot/.dotnet/sdk/2.1.300/nuGetPackagesArchive.lzma" -ea Ignore

# Build the LZMA using this ref-only package
Write-Host ""
Write-Host -f Cyan "Building a new LZMA"
Write-Host ""
$lzmaStagingDir = "$PSScriptRoot/obj/packages"
Remove-Item -Recurse -Force $lzmaStagingDir -ea ignore
Invoke-Block { & $dotnet restore src/lzma/refs/refs.csproj }
Invoke-Block { & $dotnet run -p src/lzma/redist/redist.csproj -- $lzmaStagingDir $newLzma }

$oldSize = (Get-ChildItem $bigLzma).Length
$newSize = (Get-ChildItem $newLzma).Length
Write-Host -ForegroundColor Magenta ("Old LZMA  = {0:F2} MB" -f ($oldSize/1mb))
Write-Host -ForegroundColor Magenta ("New LZMA   = {0:F2} MB" -f ($newSize/1mb))
Write-Host -ForegroundColor Magenta ("Comparison = {0:P}" -f ($newSize/$oldSize))

# Build our test mvc app, with the first-run experience enabled
Write-Host ""
Write-Host -f Cyan "Building a sample MVC app"
Write-Host ""
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 0
Remove-Item -Recurse -Force "$PSScriptRoot/.dotnet/sdk/NuGetFallbackFolder" -ea ignore
Invoke-Block { & $dotnet publish testapp/MvcWebApp/MvcWebApp.csproj -o "$PSScriptRoot/obj/publish/" }

Write-Host ""
Write-Host -f Cyan "Done"
Write-Host ""
Write-Host ""
Write-Host "You can now run a sample app by calling in `"$PSScriptRoot/obj/publish/`""
Write-Host "  cd `"$PSScriptRoot\obj\publish`""
Write-Host "  $dotnet MvcWebApp.dll"
Write-Host ""
