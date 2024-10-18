using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifySourceGenerators.Initialize();
}
