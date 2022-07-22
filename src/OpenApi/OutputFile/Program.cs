var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/students", () => "");

app.MapPost("/students/{id}", () => new string[][] {});
