// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators;

public sealed class XmlCommentParserContext
{
    public bool? SkipMarkup { get; init; }

    public Action<string, string>? AddReferenceDelegate { get; init; }

    public Func<string?, string>? ResolveCode { get; init; }

    public SourceDetail? Source { get; init; }
}
