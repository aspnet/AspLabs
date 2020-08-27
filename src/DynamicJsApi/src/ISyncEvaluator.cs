// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJs
{
    internal interface ISyncEvaluator
    {
        object Evaluate(
            long treeId,
            long targetObjectId,
            Type type,
            IJSRuntime jsRuntime,
            IEnumerable<IJSExpression> expressionList);
    }
}
