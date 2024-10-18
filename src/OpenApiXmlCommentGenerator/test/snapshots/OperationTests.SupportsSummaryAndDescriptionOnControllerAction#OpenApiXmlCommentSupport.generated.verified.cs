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

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.AspNetCore.OpenApi.SourceGenerators, Version=42.42.42.42, Culture=neutral, PublicKeyToken=adb9793829ddae60", "42.42.42.42")]
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
            _cache.Add((typeof(global::TestController), nameof(global::TestController.Get)), "{\"Summary\":\"A summary of the action.\",\"Description\":null,\"Remarks\":null,\"Returns\":null}");
            return _cache;

        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.AspNetCore.OpenApi.SourceGenerators, Version=42.42.42.42, Culture=neutral, PublicKeyToken=adb9793829ddae60", "42.42.42.42")]
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
                System.Diagnostics.Debugger.Break();
                var methodComment = JsonSerializer.Deserialize<XmlComment>(methodCommentString);
                if (methodComment is null)
                {
                    return Task.CompletedTask;
                }
                operation.Summary = methodComment.Summary;
                operation.Description = methodComment.Description;
                foreach (var parameterComment in methodComment.Parameters)
                {
                    var parameterInfo = methodInfo.GetParameters().SingleOrDefault(info => info.Name == parameterComment.Name);
                    var operationParameter = operation.Parameters?.SingleOrDefault(parameter => parameter.Name == parameterComment.Name);
                    if (operationParameter is not null)
                    {
                        operationParameter.Description = parameterComment.Description;
                        if (parameterInfo is not null)
                        {
                            operationParameter.Example = OpenApiExamplesHelper.ToOpenApiAny(parameterComment.Example, parameterInfo.ParameterType);
                        }
                    }
                    else
                    {
                        var requestBody = operation.RequestBody;
                        if (requestBody is not null)
                        {
                            requestBody.Description = parameterComment.Description;
                        }
                    }
                }
                if (methodComment.Responses is { Count: > 0} && operation.Responses is { Count: > 0 })
                {
                    foreach (var response in operation.Responses)
                    {
                        var responseComment = methodComment.Responses.SingleOrDefault(xmlResponse => xmlResponse.Code == response.Key);
                        if (responseComment is not null)
                        {
                            response.Value.Description = responseComment.Description;
                        }

                    }
                }
            }

            return Task.CompletedTask;
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.AspNetCore.OpenApi.SourceGenerators, Version=42.42.42.42, Culture=neutral, PublicKeyToken=adb9793829ddae60", "42.42.42.42")]
    file class XmlCommentSchemaTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.JsonPropertyInfo is { AttributeProvider: PropertyInfo propertyInfo })
            {
                if (XmlCommentCache.Cache.TryGetValue((propertyInfo.DeclaringType, propertyInfo.Name), out var propertyCommentString))
                {
                    var propertyComment = JsonSerializer.Deserialize<XmlComment>(propertyCommentString);
                    if (propertyComment is not null)
                    {
                        schema.Description = propertyComment.Returns ?? propertyComment.Summary;
                        if (propertyComment.Examples is { Count: > 0 })
                        {
                            schema.Example = OpenApiExamplesHelper.ToOpenApiAny(propertyComment.Examples.FirstOrDefault(), propertyInfo.PropertyType);
                        }
                    }
                }
            }
            if (XmlCommentCache.Cache.TryGetValue((context.JsonTypeInfo.Type, null), out var typeCommentString))
            {
                var typeComment = JsonSerializer.Deserialize<XmlComment>(typeCommentString);
                if (typeComment is not null)
                {
                    schema.Description = typeComment.Summary;
                    if (typeComment.Examples is { Count: > 0 })
                    {
                        schema.Example = OpenApiExamplesHelper.ToOpenApiAny(typeComment.Examples.FirstOrDefault(), context.JsonTypeInfo.Type);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.AspNetCore.OpenApi.SourceGenerators, Version=42.42.42.42, Culture=neutral, PublicKeyToken=adb9793829ddae60", "42.42.42.42")]
    file static class OpenApiExamplesHelper
    {
        public static IOpenApiAny ToOpenApiAny(string? example, Type type)
        {
            if (example is null || type is null)
            {
                return new OpenApiNull();
            }
            return Type.GetTypeCode(type) switch
            {
                TypeCode.String => new OpenApiString(example),
                TypeCode.Boolean => new OpenApiBoolean(bool.Parse(example)),
                TypeCode.Int32 => new OpenApiInteger(int.Parse(example)),
                TypeCode.Int64 => new OpenApiLong(long.Parse(example)),
                TypeCode.Double => new OpenApiDouble(double.Parse(example)),
                TypeCode.Single => new OpenApiFloat(float.Parse(example)),
                TypeCode.DateTime => new OpenApiDateTime(DateTime.Parse(example)),
                _ => new OpenApiNull()
            };
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.AspNetCore.OpenApi.SourceGenerators, Version=42.42.42.42, Culture=neutral, PublicKeyToken=adb9793829ddae60", "42.42.42.42")]
    file static class GeneratedServiceCollectionExtensions
    {
        [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "Qm3aQQrRp70T5k2GTBStnBYBAABQcm9ncmFtLmNz")]
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