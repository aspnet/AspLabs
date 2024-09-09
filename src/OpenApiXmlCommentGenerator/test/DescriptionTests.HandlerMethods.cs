using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public partial class DescriptionTests
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
}
