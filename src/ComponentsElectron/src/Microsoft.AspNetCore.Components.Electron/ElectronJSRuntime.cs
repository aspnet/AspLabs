// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ElectronNET.API;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using System;

namespace Microsoft.AspNetCore.Components.Electron
{
    internal class ElectronJSRuntime : JSRuntime
    {
        private readonly BrowserWindow _window;

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
            var resultOrError = invocationResult.Success ? invocationResult.Result : invocationResult.Exception.ToString();
            ElectronNET.API.Electron.IpcMain.Send(_window, "JS.EndInvokeDotNet", invocationInfo.CallId, invocationResult.Success, resultOrError);
        }
    }
}
