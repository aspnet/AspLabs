// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ElectronNET.API;
using Microsoft.JSInterop;
using System;

namespace Microsoft.AspNetCore.Components.Electron
{
    internal class ElectronJSRuntime : JSRuntimeBase
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
    }
}
