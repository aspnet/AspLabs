// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJS
{
    /// <summary>
    /// Contains <see cref="IJSRuntime"/> extension methods for creating root <see cref="JSObject"/> instances.
    /// </summary>
    public static class JSRuntimeExtensions
    {
        private static InProcessEvaluator? _inProcessSyncEvaluator;

        private static long _nextTreeId;

        /// <summary>
        /// Returns a <c>dynamic</c> <see cref="JSObject"/> representing the browser's <c>window</c> object.
        /// </summary>
        /// <remarks>
        /// If a relative to the returned <see cref="JSObject"/> needs to be evaluated as a .NET type, it must be done
        /// via <see cref="JSObject.EvaluateAsync{TValue}(JSObject)"/>. Attempting to directly cast a <see cref="JSObject"/>
        /// to a .NET type, either explicitly or implicitly, will throw an exception.
        /// </remarks>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> used to execute <see cref="JSObject"/> operations.</param>
        /// <returns></returns>
        public static dynamic GetDynamicWindow(this IJSRuntime jsRuntime)
        {
            return new JSExpressionTree(jsRuntime, _nextTreeId++).Root;
        }

        /// <summary>
        /// Returns a <c>dynamic</c> <see cref="JSObject"/> representing the browser's <c>window</c> object.
        /// </summary>
        /// <remarks>
        /// Operations performed involving the returned <see cref="JSObject"/> or its relatives do not need to be explicitly
        /// evaluated. Instead, they occur when <see cref="JSObject"/>s are casted to other .NET types, either
        /// explicitly or implicitly.
        /// </remarks>
        /// <param name="jsRuntime">The <see cref="IJSInProcessRuntime"/> used to execute <see cref="JSObject"/> operations.</param>
        /// <returns></returns>
        public static dynamic GetInProcessDynamicWindow(this IJSInProcessRuntime jsRuntime)
        {
            _inProcessSyncEvaluator ??= new InProcessEvaluator(jsRuntime);
            return new JSExpressionTree(jsRuntime, _nextTreeId++, _inProcessSyncEvaluator).Root;
        }
    }
}
