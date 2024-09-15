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
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Microsoft.OpenApi.Any;

    file static class XmlCommentCache
    {
        private static Dictionary<(Type?, string?), string>? _cache;
        public static Dictionary<(Type?, string?), string> Cache
        {
            get
            {
                if (_cache is null)
                {
                    _cache = GenerateCacheEntries();
                }
                return _cache;
            }
        }

        private static Dictionary<(Type?, string?), string> GenerateCacheEntries()
        {
            var _cache = new Dictionary<(Type?, string?), string>();
            _cache.Add((typeof(global::Project), null), "{\"Summary\":\"The project that contains Todo items.\",\"Description\":null,\"Remarks\":null,\"Returns\":null,\"Parameters\":[{\"Name\":\"Name\",\"Description\":\"The name of the project.\"},{\"Name\":\"Description\",\"Description\":\"The description of the project.\"}]}");
            _cache.Add((typeof(global::Project), nameof(global::Project.Name)), "{\"Summary\":\"The name of the project.\",\"Description\":null,\"Remarks\":null,\"Returns\":null}");
            _cache.Add((typeof(global::Project), nameof(global::Project.Description)), "{\"Summary\":\"The description of the project.\",\"Description\":null,\"Remarks\":null,\"Returns\":null}");
            return _cache;

        }
    }

    file class XmlCommentOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var methodInfo = context.Description.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor
                ? controllerActionDescriptor.MethodInfo
                : context.Description.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().SingleOrDefault();

            if (methodInfo is null)
            {
                return Task.CompletedTask;
            }

            if (XmlCommentCache.Cache.TryGetValue((methodInfo.DeclaringType, methodInfo.Name), out var methodCommentString))
            {
                var methodComment = JsonSerializer.Deserialize<XmlComment>(methodCommentString);
                operation.Summary = methodComment.Summary;
                operation.Description = methodComment.Description;
                if (methodComment.Parameters is { Count: > 0 })
                {
                    foreach (var parameter in operation.Parameters)
                    {
                        var parameterComment = methodComment.Parameters.SingleOrDefault(xmlParameter => xmlParameter.Name == parameter.Name);
                        parameter.Description = parameterComment.Description;
                    }
                }
                if (methodComment.Responses is { Count: > 0})
                {
                    foreach (var response in operation.Responses)
                    {
                        if (methodComment.Responses.TryGetValue(response.Key, out var responseComment))
                        {
                            response.Value.Description = responseComment.Description;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }

    file class XmlCommentSchemaTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.JsonPropertyInfo is { AttributeProvider: PropertyInfo propertyInfo })
            {
                if (XmlCommentCache.Cache.TryGetValue((propertyInfo.DeclaringType, propertyInfo.Name), out var propertyCommentString))
                {
                    var propertyComment = JsonSerializer.Deserialize<XmlComment>(propertyCommentString);
                    schema.Description = propertyComment.Returns ?? propertyComment.Summary;
                    if (propertyComment.Examples is { Count: > 0 })
                    {
                        // schema.Example = propertyComment.Examples.FirstOrDefault();
                    }
                }
            }
            if (XmlCommentCache.Cache.TryGetValue((context.JsonTypeInfo.Type, null), out var typeCommentString))
            {
                var typeComment = JsonSerializer.Deserialize<XmlComment>(typeCommentString);
                schema.Description = typeComment.Summary;
                if (typeComment.Examples is { Count: > 0 })
                {
                    // schema.Example = typeComment.Examples.FirstOrDefault();
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
                options.AddOperationTransformer(new XmlCommentOperationTransformer());
            });
        }

    }
}