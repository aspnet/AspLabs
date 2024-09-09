var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todos = app.MapGroup("/todos");

todos.MapPost("/", (Todo todo) =>
{
    return Results.Created("/todos/1", todo);
});

app.Run();

/// <summary>
/// Represents a todo item that can be created, read, updated, and deleted.
/// </summary>
public class Todo
{
    /// <summary>
    /// The main title of the todo.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Whether or not the todo has been completed.
    /// </summary>
    public bool Completed { get; set; }
}
