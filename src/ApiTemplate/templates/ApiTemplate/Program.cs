var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Open API:
// - an OpenAPI document is automatically configured at /openapi.json
// - an OpenAPI 'Swagger' UI is automatically configured at /docs

app.MapGet("/", () => "Hello World!");

app.Run();
