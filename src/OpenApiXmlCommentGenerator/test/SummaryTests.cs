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

app.MapPost("/todo", (Todo todo) => { });
app.MapPost("/project", (Project project) => { });

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

/// <summary>
/// The project that contains <see cref="Todo"/> items.
/// </summary>
/// <param name="Name">The name of the project.</param>
/// <param name="Description">The description of the project.</param>
public record Project(string Name, string Description);
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/todo"].Operations[OperationType.Post];
            var todo = path.RequestBody.Content["application/json"].Schema;
            Assert.Equal("This is a todo item.", todo.Description);
            Assert.Equal("The ID of the todo item.", todo.Properties["id"].Description);
            Assert.Equal("The name of the todo item.", todo.Properties["name"].Description);
            Assert.Equal("The description of the todo item.", todo.Properties["description"].Description);

            path = document.Paths["/project"].Operations[OperationType.Post];
            var project = path.RequestBody.Content["application/json"].Schema;
            // TODO: Fix refs in OpenAPI document.
            // Assert.Equal("The project that contains <see cref=\"Todo\"/> items.", project.Description);
            Assert.Equal("The name of the project.", project.Properties["name"].Description);
            Assert.Equal("The description of the project.", project.Properties["description"].Description);
        });
    }
}
