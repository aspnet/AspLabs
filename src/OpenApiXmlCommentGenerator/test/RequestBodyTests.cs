using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public class RequestBodyTests
{
    [Fact]
    public async Task SupportsDescriptionOnRequestBodyForMinimalApis()
    {
        // Arrange
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapPost("/", RouteHandlerExtensionMethods.Post);

app.Run();

public static class RouteHandlerExtensionMethods
{
    /// <param name="todo">The todo to insert into the database.</param>
    public static string Post(Todo todo)
    {
        return $"Hello, {todo.Title}!";
    }
}

public record Todo(int Id, string Title, bool Completed);
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/"].Operations[OperationType.Post];
            var requestBody = path.RequestBody;
            Assert.NotNull(requestBody);
            Assert.Equal("The todo to insert into the database.", requestBody.Description);
        });
    }

    [Fact]
    public async Task SupportDescriptionOnRequestBodyForControllerActions()
    {
        // Arrange
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
    /// <param name="todo">The todo to insert into the database.</param>
    [HttpPost]
    public string Post(Todo todo)
    {
        return $"Hello, {todo.Title}!";
    }
}

public record Todo(int Id, string Title, bool Completed);
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/Test"].Operations[OperationType.Post];
            var requestBody = path.RequestBody;
            Assert.NotNull(requestBody);
            Assert.Equal("The todo to insert into the database.", requestBody.Description);
        });
    }
}
