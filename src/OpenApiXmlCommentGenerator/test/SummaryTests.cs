using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public class SummaryTests
{
    [Fact]
    public async Task SupportsSummaryOnClassAndProperties()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapPost("/", (Todo todo) => { });

app.Run();

/// <summary>
/// This is a todo item.
/// </summary>
public class Todo
{
    /// <summary>
    /// The ID of the todo item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the todo item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the todo item.
    /// </summary>
    public string Description { get; set; }
}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/"].Operations[OperationType.Post];
            var todo = path.RequestBody.Content["application/json"].Schema;
            Assert.Equal("This is a todo item.", todo.Description);
            Assert.Equal("The ID of the todo item.", todo.Properties["id"].Description);
            Assert.Equal("The name of the todo item.", todo.Properties["name"].Description);
            Assert.Equal("The description of the todo item.", todo.Properties["description"].Description);
        });
    }
}
