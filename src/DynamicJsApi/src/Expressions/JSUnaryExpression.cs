// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.AspNetCore.DynamicJs
{
    internal class JSUnaryExpression : IJSExpression
    {
        public JSExpressionType Type => JSExpressionType.Unary;

        public long TargetObjectId { get; set; }

        public ExpressionType Operation { get; set; }
    }
}
