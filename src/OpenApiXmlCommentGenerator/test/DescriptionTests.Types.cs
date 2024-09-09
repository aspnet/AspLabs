using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public partial class DescriptionTests
{
    [Fact]
    public async Task SupportsDescriptionOnClasses()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapPost("/todo", (Todo todo) => { });
app.MapPost("/project", (Project project) => { });
app.MapPost("/board", (ProjectBoard.BoardItem boardItem) => { });

app.Run();

/// <summary>
/// This is a todo item.
/// </summary>
public class Todo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

/// <summary>
/// The project that contains <see cref="Todo"/> items.
/// </summary>
public record Project(string Name, string Description);

public class ProjectBoard
{
    /// <summary>
    /// An item on the board.
    /// </summary>
    public class BoardItem
    {
        public string Name { get; set; }
    }
}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/todo"].Operations[OperationType.Post];
            var todo = path.RequestBody.Content["application/json"].Schema;
            Assert.Equal("This is a todo item.", todo.Description);

            path = document.Paths["/project"].Operations[OperationType.Post];
            var project = path.RequestBody.Content["application/json"].Schema;
            // TODO: Fix refs in OpenAPI document.
            Assert.Equal("The project that contains <xref href=\"Todo\" data-throw-if-not-resolved=\"false\"></xref> items.", project.Description);

            path = document.Paths["/board"].Operations[OperationType.Post];
            var board = path.RequestBody.Content["application/json"].Schema;
            Assert.Equal("An item on the board.", board.Description);
        });
    }

    [Fact]
    public async Task SupportsDescriptionOnRecordParameters()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapPost("/project", (Project project) => { });

app.Run();

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
            var path = document.Paths["/project"].Operations[OperationType.Post];
            var project = path.RequestBody.Content["application/json"].Schema;

            Assert.Equal("The name of the project.", project.Properties["name"].Description);
            Assert.Equal("The description of the project.", project.Properties["description"].Description);
        });
    }

    [Fact]
    public async Task SupportsDescriptionOnProperties()
    {
        var source = """
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapPost("/todo", (Todo todo) => { });

app.Run();

public class Todo
{
    /// <summary>
    /// The identifier of the todo.
    /// </summary>
    public int Id { get; set; }
    /// <value>
    /// The name of the todo.
    /// </value>
    public string Name { get; set; }
    /// <summary>
    /// A description of the the todo.
    /// </summary>
    /// <value>Another description of the todo.</value>
    public string Description { get; set; }
}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/todo"].Operations[OperationType.Post];
            var todo = path.RequestBody.Content["application/json"].Schema;
            Assert.Equal("The identifier of the todo.", todo.Properties["id"].Description);
            Assert.Equal("The name of the todo.", todo.Properties["name"].Description);
            Assert.Equal("Another description of the todo.", todo.Properties["description"].Description);
        });
    }
}
