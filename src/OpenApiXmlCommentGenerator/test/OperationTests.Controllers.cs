using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public partial class OperationTests
{
    [Fact]
    public async Task SupportsSummaryAndDescriptionOnControllerAction()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(TestController).Assembly);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapControllers();

app.Run();

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// A summary of the action.
    /// </summary>
    /// <description>
    /// A description of the action.
    /// </description>
    [HttpGet]
    public string Get()
    {
        return "Hello, World!";
    }
}

public partial class Program {}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/Test"].Operations[OperationType.Get];
            Assert.Equal("A summary of the action.", path.Summary);
            // TODO: Figure out how to process the <description> tag.
            // Assert.Equal("A description of the action.", path.Description);
        });
    }

    [Fact]
    public async Task SupportsParamsAndResponseOnControllerAction()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(TestController).Assembly);

var app = builder.Build();

app.MapControllers();

app.Run();

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    /// <param name="name">The name of the person.</param>
    /// <response code="200">Returns the greeting.</response>
    [HttpGet]
    public string Get(string name)
    {
        return $"Hello, {name}!";
    }
}
""";

        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/Test"].Operations[OperationType.Get];
            Assert.Equal("The name of the person.", path.Parameters[0].Description);
            Assert.Equal("Returns the greeting.", path.Responses["200"].Description);
        });
    }
}
