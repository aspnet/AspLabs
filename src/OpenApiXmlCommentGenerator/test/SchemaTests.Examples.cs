using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public partial class SchemaTests
{
    [Fact]
    public async Task SupportsExampleOnPrimitiveTypeProperties()
    {
        var source = """
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapPost("/", (TypeWithExamples typeWithExamples) => { });

app.Run();

public class TypeWithExamples
{
    /// <example>true</example>
    public bool BooleanType { get; set; }
    /// <example>42</example>
    public int IntegerType { get; set; }
    /// <example>1234567890123456789</example>
    public long LongType { get; set; }
    /// <example>3.14</example>
    public double DoubleType { get; set; }
    /// <example>3.14</example>
    public float FloatType { get; set; }
    /// <example>2022-01-01T00:00:00Z</example>
    public DateTime DateTimeType { get; set; }
    /// <example>2022-01-01</example>
    public DateOnly DateOnlyType { get; set; }
}
""";
        var generator = new XmlCommentGenerator();
        await SnapshotTestHelper.Verify(source, generator, out var compilation);
        await SnapshotTestHelper.VerifyOpenApi(compilation, document =>
        {
            var path = document.Paths["/"].Operations[OperationType.Post];
            var typeWithExamples = path.RequestBody.Content["application/json"].Schema;

            var booleanTypeExample = Assert.IsType<OpenApiBoolean>(typeWithExamples.Properties["booleanType"].Example);
            Assert.True(booleanTypeExample.Value);

            var integerTypeExample = Assert.IsType<OpenApiInteger>(typeWithExamples.Properties["integerType"].Example);
            Assert.Equal(42, integerTypeExample.Value);

            var longTypeExample = Assert.IsType<OpenApiLong>(typeWithExamples.Properties["longType"].Example);
            Assert.Equal(1234567890123456789, longTypeExample.Value);

            var doubleTypeExample = Assert.IsType<OpenApiDouble>(typeWithExamples.Properties["doubleType"].Example);
            Assert.Equal(3.14, doubleTypeExample.Value);

            var floatTypeExample = Assert.IsType<OpenApiFloat>(typeWithExamples.Properties["floatType"].Example);
            Assert.Equal(3.14f, floatTypeExample.Value);

            var dateTimeTypeExample = Assert.IsType<OpenApiDateTime>(typeWithExamples.Properties["dateTimeType"].Example);
            Assert.Equal(DateTime.Parse("2022-01-01T00:00:00Z"), dateTimeTypeExample.Value);

            var dateOnlyTypeExample = Assert.IsType<OpenApiDate>(typeWithExamples.Properties["dateOnlyType"].Example);
            Assert.Equal(DateTime.Parse("2022-01-01"), dateOnlyTypeExample.Value);
        });
    }
}
