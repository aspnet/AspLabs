// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.AspNetCore.DynamicJs
{
    internal class JSBinaryExpression : IJSExpression
    {
        public JSExpressionType Type => JSExpressionType.Binary;

        public long TargetObjectId { get; set; }

        public ExpressionType Operation { get; set; }

        public object? Arg { get; set; }
    }
}
