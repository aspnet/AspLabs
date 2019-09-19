// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.Components.Electron
{
    /// <summary>
    /// Provides an implementation of <see cref="IUriHelper"/> that interacts with an Electron shell.
    /// </summary>
    public class ElectronNavigationManager : NavigationManager
    {
        private static readonly string InteropPrefix = "Blazor._internal.uriHelper.";
        private static readonly string InteropNavigateTo = InteropPrefix + "navigateTo";

        /// <summary>
        /// Gets an instance of <see cref="ElectronNavigationManager"/>.
        /// </summary>
        public static ElectronNavigationManager Instance { get; } = new ElectronNavigationManager();

        private ElectronNavigationManager()
        {
        }

        protected override void EnsureInitialized()
        {
            Initialize(Launcher.BaseUriAbsolute, Launcher.InitialUriAbsolute);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Launcher.ElectronJSRuntime.InvokeAsync<object>(InteropNavigateTo, uri, forceLoad);
        }
    }
}
