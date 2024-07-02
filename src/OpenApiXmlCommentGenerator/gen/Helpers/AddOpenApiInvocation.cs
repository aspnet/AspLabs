// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators;

internal record AddOpenApiInvocation(
    AddOpenApiOverloadVariant Variant,
    InvocationExpressionSyntax InvocationExpression,
    InterceptableLocation? Location);
