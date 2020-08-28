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

        public static dynamic GetWindow(this IJSRuntime jsRuntime)
        {
            if (jsRuntime is IJSInProcessRuntime jsInProcessRuntime)
            {
                _syncEvaluator ??= new BrowserSyncEvaluator(jsInProcessRuntime);
                return new JSExpressionTree(_syncEvaluator, _nextTreeId++).Root;
            }

            throw new InvalidOperationException(
                $"Can only be called on a {typeof(IJSInProcessRuntime)} instance. " +
                $"Use {nameof(GetWindowAsync)} when using Blazor Server.");
        }

        public static ValueTask<dynamic> GetWindowAsync(this IJSRuntime jsRuntime)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
