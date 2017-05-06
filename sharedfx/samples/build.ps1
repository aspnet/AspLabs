$ErrorActionPreference = 'Stop'

# Copy to the 'local' hive just for demoing
if (!(Test-Path $PSScriptRoot/publish/shared)) {
    mkdir $PSScriptRoot/publish/ -ErrorAction Ignore | Out-Null
    copy-item -Recurse $PSScriptRoot/../artifacts/shared $PSScriptRoot/publish/
}

dotnet restore Web/
dotnet publish Web/ -c Release -o $PSScriptRoot/publish/

push-location $PSScriptRoot/publish/
try {
    dotnet Web.dll
} finally {
    pop-location
}
