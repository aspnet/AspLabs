## Running this script will update the type forwards and .NET Standard APIs

dotnet build .\System.Web.Adapters.csproj -f netcoreapp3.1 /p:GenerateStandard=true --no-incremental
dotnet build .\System.Web.Adapters.csproj -f netcoreapp3.1 /p:GenerateTypeForwards=true --no-incremental

# Script will have an error if there are git changes
if(git status --porcelain){
  exit 1
}
