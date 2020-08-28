// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.DynamicJS
{
    internal class JSAssignmentExpression : IJSExpression
    {
        public JSExpressionType Type => JSExpressionType.Assignment;

        public long TargetObjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public object? Value { get; set; }
    }
}
