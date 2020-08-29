// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJS
{
    public static class JSRuntimeExtensions
    {
        private static long _nextTreeId;

        private static ISyncEvaluator? _syncEvaluator;

        public static dynamic GetWindowDynamic(this IJSInProcessRuntime jsRuntime)
        {
            _syncEvaluator ??= new BrowserSyncEvaluator(jsRuntime);
            return new JSExpressionTree(_syncEvaluator, _nextTreeId++).Root;
        }

        public static void EvaluateDynamic(this IJSInProcessRuntime jsRuntime, JSObject jsObject)
        {

        }

        public static ValueTask<dynamic> GetWindowDynamicAsync(this IJSRuntime jsRuntime)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
