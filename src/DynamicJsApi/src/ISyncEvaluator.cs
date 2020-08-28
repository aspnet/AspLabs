// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.DynamicJS
{
    internal interface ISyncEvaluator
    {
        object Evaluate(
            Type returnType,
            long treeId,
            long targetObjectId,
            IEnumerable<IJSExpression> expressionList);
    }
}
