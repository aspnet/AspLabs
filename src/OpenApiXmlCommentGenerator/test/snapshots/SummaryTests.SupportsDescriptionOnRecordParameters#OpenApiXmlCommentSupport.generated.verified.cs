﻿//HintName: OpenApiXmlCommentSupport.generated.cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

namespace System.Runtime.CompilerServices
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.AspNetCore.OpenApi.SourceGenerators, Version=42.42.42.42, Culture=neutral, PublicKeyToken=adb9793829ddae60", "42.42.42.42")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute : System.Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
        }
    }
}

namespace Microsoft.AspNetCore.OpenApi.Generated
{
    using DocFx.XmlComments;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.OpenApi;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Microsoft.OpenApi.Any;

    file class XmlComment
    {
        public string? Summary { get; set; }
        public IOpenApiAny? Example { get; set; }
    }

    file class XmlCommentSchemaTransformer : IOpenApiSchemaTransformer
    {
        private readonly Dictionary<(Type?, string?), XmlComment> _cache = new();
        public XmlCommentSchemaTransformer()
        {
            XmlComment xmlComment;
            xmlComment = new XmlComment();
            xmlComment.Summary = "The project that contains Todo items.";
            _cache.Add((typeof(global::Project), null), xmlComment);
            xmlComment = new XmlComment();
            xmlComment.Summary = "The name of the project.";
            _cache.Add((typeof(global::Project), nameof(global::Project.Name)), xmlComment);
            xmlComment = new XmlComment();
            xmlComment.Summary = "The description of the project.";
            _cache.Add((typeof(global::Project), nameof(global::Project.Description)), xmlComment);

        }

        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.JsonPropertyInfo is { AttributeProvider: PropertyInfo propertyInfo })
            {
                if (_cache.TryGetValue((propertyInfo.DeclaringType, propertyInfo.Name), out var propertyComment))
                {
                    schema.Description = propertyComment.Summary;
                    if (propertyComment.Example is not null)
                    {
                        schema.Example = propertyComment.Example;
                    }
                }
            }
            if (_cache.TryGetValue((context.JsonTypeInfo.Type, null), out var typeComment))
            {
                schema.Description = typeComment.Summary;
                if (schema.Example is not null)
                {
                    schema.Example = typeComment.Example;
                }
            }
            return Task.CompletedTask;
        }
    }

    file static class GeneratedServiceCollectionExtensions
    {
        [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "GnlWzMNYDklpsHJgqq5yYJUAAABQcm9ncmFtLmNz")]
        public static IServiceCollection AddOpenApi(this IServiceCollection services)
        {
            return services.AddOpenApi("v1", options =>
            {
                options.AddSchemaTransformer(new XmlCommentSchemaTransformer());
            });
        }

    }
}