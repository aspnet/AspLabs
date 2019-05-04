// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.Components.Electron
{
    /// <summary>
    /// Contains methods for starting an Electron process and attaching components.
    /// </summary>
    public static class ComponentsElectron
    {
        /// <summary>
        /// Starts an Electron process using the specified JavaScript file as the host entrypoint.
        /// </summary>
        public static void Run<TStartup>(string hostJsPath)
        {
            Launcher.StartElectronProcess(async () =>
            {
                var window = await Launcher.CreateWindowAsync(hostJsPath);
                JSRuntime.SetCurrentJSRuntime(Launcher.ElectronJSRuntime);

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton<IUriHelper>(ElectronUriHelper.Instance);
                serviceCollection.AddSingleton<IJSRuntime>(Launcher.ElectronJSRuntime);

                var startup = new ConventionBasedStartup(Activator.CreateInstance(typeof(TStartup)));
                startup.ConfigureServices(serviceCollection);

                var services = serviceCollection.BuildServiceProvider();
                var builder = new ElectronApplicationBuilder(services);
                startup.Configure(builder, services);

                ElectronUriHelper.Instance.InitializeState(
                    Launcher.InitialUriAbsolute,
                    Launcher.BaseUriAbsolute);

                var renderer = new ElectronRenderer(services, window);
                foreach (var rootComponent in builder.Entries)
                {
                    _ = renderer.AddComponentAsync(rootComponent.componentType, rootComponent.domElementSelector);
                }
            });
        }
    }
}
