using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public partial class AddOpenApiTests
{
    [Fact]
    public async Task CanInterceptAddOpenApiWithNoParameters()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapPost("", () => "Hello world!");

app.Run();
""";

        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity > DiagnosticSeverity.Warning));
    }

    [Fact]
    public async Task CanInterceptAddOpenApiWithNameParameter()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi("v2");

var app = builder.Build();

app.MapPost("", () => "Hello world!");

app.Run();
""";

        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity > DiagnosticSeverity.Warning));
    }

    [Fact]
    public async Task CanInterceptAddOpenApiWithConfigureOptionsParameter()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
});

var app = builder.Build();

app.MapPost("", () => "Hello world!");

app.Run();
""";

        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity > DiagnosticSeverity.Warning));
    }

        [Fact]
    public async Task CanInterceptAddOpenApiWithNameAndConfigureOptionsParameter()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi("v2", options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
});

var app = builder.Build();

app.MapPost("", () => "Hello world!");

app.Run();
""";

        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity > DiagnosticSeverity.Warning));
    }
}
