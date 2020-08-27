// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.DynamicJs
{
    internal class JSMethodExpression : IJSExpression
    {
        public JSExpressionType Type => JSExpressionType.Method;

        public long TargetObjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public object?[]? Args { get; set; }
    }
}
