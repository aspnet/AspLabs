// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators;

enum SymbolUrlKind
{
    Html,
    Markdown,
}

internal static partial class SymbolUrlResolver
{
    public static string? GetSymbolUrl()
    {
        return string.Empty;
    }

    public static string GetLangwordUrl(string langWord) => langWord;
}
