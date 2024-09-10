using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public partial class OperationTests
{
    [Fact]
    public async Task SupportsSummaryAndDescriptionOnHandlerMethods()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/", RouteHandlerExtensionMethods.Get);

app.Run();

public static class RouteHandlerExtensionMethods
{
    /// <summary>
    /// A summary of the action.
    /// </summary>
    /// <description>
    /// A description of the action.
    /// </description>
    public static string Get()
    {
        return "Hello, World!";
    }
}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/"].Operations[OperationType.Get];
            Assert.Equal("A summary of the action.", path.Summary);
            // TODO: Figure out how to process the <description> tag.
            // Assert.Equal("A description of the action.", path.Description);
        });
    }

    [Fact]
    public async Task SupportsParamsAndResponseOnHandlerMethods()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

var app = builder.Build();

app.MapGet("/", RouteHandlerExtensionMethods.Get);

app.Run();

public static class RouteHandlerExtensionMethods
{
    /// <param name="name">The name of the person.</param>
    /// <response code="200">Returns the greeting.</response>
    public static string Get(string name)
    {
        return $"Hello, {name}!";
    }
}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/"].Operations[OperationType.Get];
            Assert.Equal("The name of the person.", path.Parameters[0].Description);
            Assert.Equal("Returns the greeting.", path.Responses["200"].Description);
        });
    }

    [Fact]
    public async Task SupportsParamsWithExampleOnHandlerMethods()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

var app = builder.Build();

app.MapGet("/", RouteHandlerExtensionMethods.Get);

app.Run();

public static class RouteHandlerExtensionMethods
{
    /// <param name="name" example="Testy McTester">The name of the person.</param>
    public static string Get(string name)
    {
        return $"Hello, {name}!";
    }
}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/"].Operations[OperationType.Get];
            Assert.Equal("The name of the person.", path.Parameters[0].Description);
            var example = Assert.IsType<OpenApiString>(path.Parameters[0].Example);
            Assert.Equal("Testy McTester", example.Value);
        });
    }

    [Fact]
    public async Task SupportsResponseWithCustomStatusCodeOnHandlerMethods()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

var app = builder.Build();

app.MapGet("/", RouteHandlerExtensionMethods.Get);

app.Run();

public static class RouteHandlerExtensionMethods
{
    /// <response name="404">Indicates that the value was not found.</param>
    public static IResult Get()
    {
        return TypedResults.NotFound("Not found!");
    }
}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/"].Operations[OperationType.Get];
            var response = path.Responses["404"];
            Assert.Equal("Indicates that the value was not found.", response.Description);
        });
    }
}
