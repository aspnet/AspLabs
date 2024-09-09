using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators.Tests;

public class ExampleTests
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
    /// <example>3.14f</example>
    public float FloatType { get; set; }
    /// <example>3.14</example>
    // public decimal DecimalType { get; set; }
    /// <example>2022-01-01T00:00:00Z</example>
    public DateTime DateTimeType { get; set; }
    /// <example>2022-01-01T00:00:00Z</example>
    public DateTimeOffset DateTimeOffsetType { get; set; }
    /// <example>2022-01-01T00:00:00Z</example>
    public TimeSpan TimeSpanType { get; set; }
    /// <example>SGVsbG8gV29ybGQ=</example>
    public byte[] ByteArrayType { get; set; }
    /// <example>{ "a" : 1, "b": 3.14, "c": "hello" }</example>
    public object ObjectType { get; set; }
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

            // var decimalTypeExample = Assert.IsType<OpenApi>(typeWithExamples.Properties["decimalType"].Example);
            // Assert.Equal(3.14m, decimalTypeExample.Value);

            var dateTimeTypeExample = Assert.IsType<OpenApiDateTime>(typeWithExamples.Properties["dateTimeType"].Example);
            Assert.Equal(DateTime.Parse("2022-01-01T00:00:00Z"), dateTimeTypeExample.Value);

            // var dateTimeOffsetTypeExample = Assert.IsType<OpenApiDateTimeOffset>(typeWithExamples.Properties["dateTimeOffsetType"].Example);
            // Assert.Equal(DateTimeOffset.Parse("2022-01-01T00:00:00Z"), dateTimeOffsetTypeExample.Value);

            // var timeSpanTypeExample = Assert.IsType<OpenApiTimeSpan>(typeWithExamples.Properties["timeSpanType"].Example);
            // Assert.Equal(TimeSpan.Parse("00:00:00"), timeSpanTypeExample.Value);

            // var byteArrayTypeExample = Assert.IsType<OpenApiBinary>(typeWithExamples.Properties["byteArrayType"].Example);
            // Assert.Equal("SGVsbG8gV29ybGQ=", byteArrayTypeExample.Value);

            // var objectTypeExample = Assert.IsType<OpenApiObject>(typeWithExamples.Properties["objectType"].Example);
            // Assert.Equal(1, Assert.IsType<OpenApiInteger>(objectTypeExample["a"]).Value);
            // Assert.Equal(3.14, Assert.IsType<OpenApiDouble>(objectTypeExample["b"]).Value);
            // Assert.Equal("hello", Assert.IsType<OpenApiString>(objectTypeExample["c"]).Value);
        });
    }
}
