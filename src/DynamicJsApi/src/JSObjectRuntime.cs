// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJs
{
    public class JSObjectRuntime
    {
        private readonly IJSRuntime _jsRuntime;

        private readonly ISyncEvaluator _syncEvaluator;

        private long _nextTreeId;

        public JSObjectRuntime(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _syncEvaluator = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")) ?
                new BrowserSyncEvaluator() :
                throw new PlatformNotSupportedException("Not yet at least");
        }

        public dynamic GetWindow()
        {
            var expressionTree = new JSExpressionTree(this, _nextTreeId++);
            return expressionTree.Root;
        }

        internal object? Evaluate(
            long treeId,
            long targetObjectId,
            Type type,
            IEnumerable<IJSExpression> expressionList)
            => _syncEvaluator.Evaluate(treeId, targetObjectId, type, _jsRuntime, expressionList);
    }
}
