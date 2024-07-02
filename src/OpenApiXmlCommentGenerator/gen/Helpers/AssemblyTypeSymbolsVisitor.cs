// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators;

internal class AssemblyTypeSymbolsVisitor : SymbolVisitor
{
    private readonly CancellationToken _cancellationToken;
    private readonly HashSet<INamedTypeSymbol> _exportedTypes;
    private readonly HashSet<IPropertySymbol> _exportedProperties;
    private readonly HashSet<IMethodSymbol> _exportedMethods;

    public AssemblyTypeSymbolsVisitor(CancellationToken cancellation)
    {
        _cancellationToken = cancellation;
        _exportedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        _exportedProperties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        _exportedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
    }

    public ImmutableArray<INamedTypeSymbol> GetPublicTypes() => [.. _exportedTypes];
    public ImmutableArray<IPropertySymbol> GetPublicProperties() => [.. _exportedProperties];
    public ImmutableArray<IMethodSymbol> GetPublicMethods() => [.. _exportedMethods];

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        symbol.GlobalNamespace.Accept(this);
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (INamespaceOrTypeSymbol namespaceOrType in symbol.GetMembers())
        {
            _cancellationToken.ThrowIfCancellationRequested();
            namespaceOrType.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol type)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        if (!type.IsAccessibleOutsideOfAssembly() || !_exportedTypes.Add(type))
        {
            return;
        }

        var nestedTypes = type.GetTypeMembers();

        foreach (var nestedType in nestedTypes)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            nestedType.Accept(this);
        }

        var properties = type.GetMembers().OfType<IPropertySymbol>();
        foreach (var property in properties)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (property.IsAccessibleOutsideOfAssembly() && _exportedProperties.Add(property))
            {
                property.Type.Accept(this);
            }
        }
        var methods = type.GetMembers().OfType<IMethodSymbol>();
        foreach (var method in methods)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (method.IsAccessibleOutsideOfAssembly() && _exportedMethods.Add(method))
            {
                method.Accept(this);
            }
        }
    }
}
