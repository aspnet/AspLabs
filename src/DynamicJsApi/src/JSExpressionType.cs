// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.DynamicJs
{
    internal enum JSExpressionType
    {
        Property = 0,
        Method = 1,
        Invocation = 2,
        Instantiation = 3,
        Assignment = 4,
        Binary = 5,
        Unary = 6,
    }
}
