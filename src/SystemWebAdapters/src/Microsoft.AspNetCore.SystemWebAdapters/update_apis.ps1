## Running this script will update the type forwards and .NET Standard APIs

dotnet build --no-incremental .\Microsoft.AspNetCore.SystemWebAdapters.csproj -f netcoreapp3.1 /p:GenerateStandard=true
dotnet build --no-incremental .\Microsoft.AspNetCore.SystemWebAdapters.csproj -f netcoreapp3.1 /p:GenerateTypeForwards=true

# Script will have an error if there are git changes
if(git status --porcelain){
  exit 1
}
