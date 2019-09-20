// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ElectronNET.API;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.Electron
{
    internal class ElectronJSRuntime : JSRuntime
    {
        private readonly BrowserWindow _window;
        private static Type VoidTaskResultType = typeof(Task).Assembly
            .GetType("System.Threading.Tasks.VoidTaskResult", true);

        public ElectronJSRuntime(BrowserWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        protected override void BeginInvokeJS(long asyncHandle, string identifier, string argsJson)
        {
            ElectronNET.API.Electron.IpcMain.Send(_window, "JS.BeginInvokeJS", asyncHandle, identifier, argsJson);
        }

        protected override void EndInvokeDotNet(DotNetInvocationInfo invocationInfo, in DotNetInvocationResult invocationResult)
        {
            // The other params aren't strictly required and are only used for logging
            var resultOrError = invocationResult.Success ? HandlePossibleVoidTaskResult(invocationResult.Result) : invocationResult.Exception.ToString();
            if (resultOrError != null)
            {
                ElectronNET.API.Electron.IpcMain.Send(_window, "JS.EndInvokeDotNet", invocationInfo.CallId, invocationResult.Success, resultOrError);
            }
            else
            {
                ElectronNET.API.Electron.IpcMain.Send(_window, "JS.EndInvokeDotNet", invocationInfo.CallId, invocationResult.Success);
            }
        }

        private static object HandlePossibleVoidTaskResult(object result)
        {
            // Looks like the TaskGenericsUtil logic in Microsoft.JSInterop doesn't know how to
            // understand System.Threading.Tasks.VoidTaskResult
            return result?.GetType() == VoidTaskResultType ? null : result;
        }
    }
}
