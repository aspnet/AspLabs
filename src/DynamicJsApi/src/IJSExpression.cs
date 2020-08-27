// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.DynamicJs
{
    internal interface IJSExpression
    {
        JSExpressionType Type { get; }

        long TargetObjectId { get; set; }
    }
}
