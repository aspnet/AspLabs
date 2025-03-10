// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.CodeAnalysis;

public static class INamedTypeSymbolExtensions
{
    public static bool IsAccessibleOutsideOfAssembly(this ISymbol symbol) =>
        symbol.DeclaredAccessibility switch
        {
            Accessibility.Private => false,
            Accessibility.Internal => false,
            Accessibility.ProtectedAndInternal => false,
            Accessibility.Protected => true,
            Accessibility.ProtectedOrInternal => true,
            Accessibility.Public => true,
            _ => true,    //Here should be some reasonable default
        };

    /// <summary>
    /// Converts a type symbol to its normalized display string representation. For generic types, this returns the
    /// unbounded generic form (e.g., "IMarker` `" instead of "IMarker`T`"). For non-generic types, returns the
    /// standard display string.
    /// </summary>
    /// <param name="symbol">The type symbol to convert</param>
    /// <param name="format">Optional display format settings</param>
    /// <returns>A normalized string representation of the type</returns>
    public static string ToNormalizedDisplayString(this INamedTypeSymbol symbol, SymbolDisplayFormat? format = null)
    {
        if (symbol == null)
            throw new ArgumentNullException(nameof(symbol));

        if (symbol.IsGenericType)
        {
            var genericType = symbol.ConstructUnboundGenericType();
            return genericType.ToDisplayString(format);
        }

        return symbol.ToDisplayString(format);
    }
}
