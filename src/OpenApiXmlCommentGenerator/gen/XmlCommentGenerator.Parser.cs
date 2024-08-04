// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DocFx.XmlComments;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators;

public sealed partial class XmlCommentGenerator
{
    internal static IEnumerable<(string, string?, XmlComment?)> ParseComments(Compilation compilation, CancellationToken cancellationToken)
    {
        var visitor = new AssemblyTypeSymbolsVisitor(cancellationToken);
        visitor.VisitAssembly(compilation.Assembly);
        var types = visitor.GetPublicTypes();
        var comments = new List<(string, string?, XmlComment?)>();
        foreach (var type in types)
        {
            var comment = type.GetDocumentationComment(
                compilation: compilation,
                preferredCulture: CultureInfo.InvariantCulture,
                expandIncludes: true,
                expandInheritdoc: true,
                cancellationToken: cancellationToken);
            if (!string.IsNullOrEmpty(comment) && !string.Equals("<doc />", comment, StringComparison.Ordinal))
            {
                var typeInfo = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                comments.Add((typeInfo, null, XmlComment.Parse(comment, new())));
            }
        }
        var properties = visitor.GetPublicProperties();
        foreach (var property in properties)
        {
            var comment = property.GetDocumentationComment(
                compilation: compilation,
                preferredCulture: CultureInfo.InvariantCulture,
                expandIncludes: true,
                expandInheritdoc: true,
                cancellationToken: cancellationToken);
            if (!string.IsNullOrEmpty(comment) && !string.Equals("<doc />", comment, StringComparison.Ordinal))
            {
                var typeInfo = property.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var propertyInfo = property.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                comments.Add((typeInfo, propertyInfo, XmlComment.Parse(comment, new())));
            }
        }
        var methods = visitor.GetPublicMethods();
        foreach (var method in methods)
        {
            var comment = method.GetDocumentationComment(
                compilation: compilation,
                preferredCulture: CultureInfo.InvariantCulture,
                expandIncludes: true,
                expandInheritdoc: true,
                cancellationToken: cancellationToken);
            if (!string.IsNullOrEmpty(comment) && !string.Equals("<doc />", comment, StringComparison.Ordinal))
            {
                // If the method is a constructor for a record, skip it because we will have already processed the type.
                if (method.MethodKind == MethodKind.Constructor)
                {
                    continue;
                }
                var typeInfo = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var methodInfo = method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                comments.Add((typeInfo, methodInfo, XmlComment.Parse(comment, new())));
            }
        }
        return comments;
    }

    internal static bool FilterInvocations(SyntaxNode node, CancellationToken _)
        => node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "AddOpenApi" } };

    internal static AddOpenApiInvocation GetAddOpenApiOverloadVariant(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;
        var interceptableLocation = context.SemanticModel.GetInterceptableLocation(invocationExpression, cancellationToken);
        return new(invocationExpression.ArgumentList.Arguments.Count switch
        {
            0 => AddOpenApiOverloadVariant.AddOpenApi,
            1 => AddOpenApiOverloadVariant.AddOpenApiDocumentName,
            2 => AddOpenApiOverloadVariant.AddOpenApiDocumentNameConfigureOptions,
            _ => throw new InvalidOperationException("Invalid number of arguments for supported `AddOpenApi` overload."),
        }, invocationExpression, interceptableLocation);
    }
}
