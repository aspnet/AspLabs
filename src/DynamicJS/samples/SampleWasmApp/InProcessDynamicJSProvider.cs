// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DynamicJS;
using Microsoft.JSInterop;

namespace SampleWasmApp
{
    public class InProcessDynamicJSProvider : IDynamicJSProvider
    {
        private readonly IJSInProcessRuntime _jsRuntime;

        public InProcessDynamicJSProvider(IJSInProcessRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Task<object> Evaluate<TValue>(dynamic jsObject)
        {
            return Task.FromResult((object)(TValue)jsObject);
        }

        public Task<object> RunWithWindow(Func<JSObject, Task<object>> operation)
        {
            using var window = _jsRuntime.GetInProcessDynamicWindow();
            return operation(window);
        }
    }
}
