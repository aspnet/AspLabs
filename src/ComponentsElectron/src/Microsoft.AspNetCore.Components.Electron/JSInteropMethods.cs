// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.Components.Electron
{
    public static class JSInteropMethods
    {
        [JSInvokable(nameof(DispatchEvent))]
        public static async Task DispatchEvent(WebEventDescriptor eventDescriptor, string eventArgsJson)
        {
            var webEvent = WebEventData.Parse(eventDescriptor, eventArgsJson);
            var renderer = Launcher.ElectronRenderer;
            await renderer.DispatchEventAsync(
                webEvent.EventHandlerId,
                webEvent.EventFieldInfo,
                webEvent.EventArgs);
        }

        [JSInvokable(nameof(NotifyLocationChanged))]
        public static void NotifyLocationChanged(string uri, bool isInterceptedLink)
        {
            ElectronNavigationManager.Instance.SetLocation(uri, isInterceptedLink);
        }
    }
}
