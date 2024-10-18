```markdown
# XML Documentation Comment Support for OpenAPI (Experimental Preview)

This package provides functionality to process XML documentation comments in C# source code and integrate them into Open API documents generated via the [Microsoft.AspNetCore.OpenApi package](). This allows for enriched API documentation with detailed descriptions, summaries, and examples directly sourced from your code comments.

## Features

- **XML Comment Parsing**: Parses XML documentation comments from C# source files.
- **OpenAPI Integration**: Integrates parsed comments into OpenAPI documents.
- **Schema Transformation**: Maps XML comments to OpenAPI schema descriptions and examples.
- **High Performance**: Designed for high performance and native AoT-friendly environments.

## Installation

To install the package, use the following command:

```sh
dotnet add package Microsoft.AspNetCore.OpenApi.SourceGenerators --prerelease
```

## Usage

### Setting Up

1. **Add OpenAPI Services**: Register OpenAPI services in your application.

    ```csharp
    var builder = WebApplication.CreateBuilder();
    builder.Services.AddOpenApi();
    var app = builder.Build();

    if (app.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.Run();
    ```

2. **Map Endpoints**: Define your API endpoints.

    ```csharp
    app.MapPost("/todo", (Todo todo) => TypedResults.Ok(todo));
    ```

3. **Add XML Comments**: Add XML documentation comments to your classes and methods.

    ```csharp
    /// <summary>
    /// Represents a todo item.
    /// </summary>
    class Todo
    {
        /// <summary>
        /// Represents a unique identifier associated with the todo.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Represents the title of the todo.
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// Represents whether or not the task is completed.
        /// </summary>
        public bool IsCompleted { get; init; }
    }
    ```

### Generating OpenAPI Document

The package will automatically integrate the XML comments into the OpenAPI document. It is not necessary to enable any options to support this.

### Example

Given the following XML comments:

```csharp
/// <summary>
/// Represents a todo item.
/// </summary>
class Todo
{
    /// <summary>
    /// Represents a unique identifier associated with the todo.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Represents the title of the todo.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Represents whether or not the task is completed.
    /// </summary>
    public bool IsCompleted { get; init; }
}
```

The generated OpenAPI schema will include these descriptions:

```json
{
  "Todo": {
    "type": "object",
    "properties": {
      "Id": {
        "type": "integer",
        "description": "Represents a unique identifier associated with the todo."
      },
      "Title": {
        "type": "string",
        "description": "Represents the title of the todo."
      },
      "IsCompleted": {
        "type": "boolean",
        "description": "Represents whether or not the task is completed."
      }
    },
    "description": "Represents a todo item."
  }
}
```

## Experimental Preview

This package is currently in experimental preview. Please file any issues discovered in this package in the [dotnet/aspnetcore repository](https://github.com/dotnet/aspnetcore/issues) for further review.

