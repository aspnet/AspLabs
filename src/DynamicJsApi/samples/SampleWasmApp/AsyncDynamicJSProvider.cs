// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DynamicJS;
using Microsoft.JSInterop;

namespace SampleWasmApp
{
    public class AsyncDynamicJSProvider : IDynamicJSProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public AsyncDynamicJSProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<object> Evaluate<TValue>(dynamic jsObject)
        {
            var result = await JSObject.EvaluateAsync<TValue>(jsObject);
            return (object)result;
        }

        public async Task<object> RunWithWindow(Func<JSObject, Task<object>> operation)
        {
            await using var window = _jsRuntime.GetDynamicWindow();
            return await operation(window);
        }
    }
}
