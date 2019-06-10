// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.Components.Electron
{
    /// <summary>
    /// Provides an implementation of <see cref="IUriHelper"/> that interacts with an Electron shell.
    /// </summary>
    public class ElectronUriHelper : UriHelperBase
    {
        private static readonly string InteropPrefix = "Blazor._internal.uriHelper.";
        private static readonly string InteropEnableNavigationInterception = InteropPrefix + "enableNavigationInterception";
        private static readonly string InteropNavigateTo = InteropPrefix + "navigateTo";

        /// <summary>
        /// Gets an instance of <see cref="ElectronUriHelper"/>.
        /// </summary>
        public static ElectronUriHelper Instance { get; } = new ElectronUriHelper();

        private ElectronUriHelper()
        {
        }

        public override void InitializeState(string uriAbsolute, string baseUriAbsolute)
        {
            base.InitializeState(uriAbsolute, baseUriAbsolute);

            SetAbsoluteBaseUri(baseUriAbsolute);
            SetAbsoluteUri(uriAbsolute);
            TriggerOnLocationChanged(false);

            Launcher.ElectronJSRuntime.InvokeAsync<object>(
                InteropEnableNavigationInterception,
                typeof(ElectronUriHelper).Assembly.GetName().Name,
                nameof(NotifyLocationChanged));
        }

        /// <summary>
        /// Receives notifications from the Electron shell that a navigation event has occurred.
        /// </summary>
        [JSInvokable(nameof(NotifyLocationChanged))]
        public static void NotifyLocationChanged(string uriAbsolute)
        {
            Instance.SetAbsoluteUri(uriAbsolute);
            Instance.TriggerOnLocationChanged(false);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Launcher.ElectronJSRuntime.InvokeAsync<object>(InteropNavigateTo, uri, forceLoad);
        }
    }
}
