// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.DynamicJs
{
    internal class JSInstantiationExpression : IJSExpression
    {
        public JSExpressionType Type => JSExpressionType.Instantiation;

        public long TargetObjectId { get; set; }

        public object? Value { get; set; }
    }
}
