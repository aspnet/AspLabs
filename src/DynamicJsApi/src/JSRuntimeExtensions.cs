// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJS
{
    public static class JSRuntimeExtensions
    {
        private static InProcessEvaluator? _inProcessSyncEvaluator;

        private static long _nextTreeId;

        public static dynamic GetInProcessDynamicWindow(this IJSInProcessRuntime jsRuntime)
        {
            _inProcessSyncEvaluator ??= new InProcessEvaluator(jsRuntime);
            return new JSExpressionTree(jsRuntime, _nextTreeId++, _inProcessSyncEvaluator).Root;
        }

        public static dynamic GetDynamicWindow(this IJSRuntime jsRuntime)
        {
            return new JSExpressionTree(jsRuntime, _nextTreeId++).Root;
        }
    }
}
