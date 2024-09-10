var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todos = app.MapGroup("/todos");

todos.MapPost("/", RouteHandlerExtensionMethods.PostTodo);

app.Run();

public static class RouteHandlerExtensionMethods
{
    /// <summary>
    /// Create a new Todo item.
    /// </summary>
    /// <param name="todo">The todo item to create.</param>
    /// <response code="201">The created Todo item.</response>
    public static IResult PostTodo(Todo todo)
    {
        return TypedResults.Created("/todo/1", todo);
    }
}

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
