// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using ElectronNET.API;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Components.Browser;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.JSInterop;

// Many aspects of the layering here are not what we really want, but it won't affect
// people prototyping applications with it. We can put more work into restructuring the
// hosting and startup models for Electron in the future if it's justified.

namespace Microsoft.AspNetCore.Components.Electron
{
    internal class ElectronRenderer : Renderer
    {
        private readonly BrowserWindow _window;
        public int RendererId { get; }

        static Func<Renderer, int> _addToRendererRegistry;

        static ElectronRenderer()
        {
            var resolverType = typeof(ComponentHub).Assembly
                .GetType("Microsoft.AspNetCore.Components.Server.Circuits.RenderBatchFormatterResolver", true);
            var resolver = (IFormatterResolver)Activator.CreateInstance(resolverType);
            CompositeResolver.RegisterAndSetAsDefault(resolver, StandardResolver.Instance);

            // Need to access Microsoft.AspNetCore.Components.Browser.RendererRegistry.Current.Add
            var rendererRegistryType = typeof(RendererRegistryEventDispatcher).Assembly
                .GetType("Microsoft.AspNetCore.Components.Browser.RendererRegistry", true);
            var rendererRegistryCurrent = rendererRegistryType
                    .GetProperty("Current", BindingFlags.Static | BindingFlags.Public)
                    .GetValue(null);
            var rendererRegistryAddMethod = rendererRegistryType
                .GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
            _addToRendererRegistry = renderer => (int)rendererRegistryAddMethod.Invoke(rendererRegistryCurrent, new[] { renderer });
        }

        public ElectronRenderer(IServiceProvider serviceProvider, BrowserWindow window)
            : base(serviceProvider)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            RendererId = _addToRendererRegistry(this);
        }

        /// <summary>
        /// Notifies when a rendering exception occured.
        /// </summary>
        public event EventHandler<Exception> UnhandledException;

        /// <summary>
        /// Attaches a new root component to the renderer,
        /// causing it to be displayed in the specified DOM element.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component.</typeparam>
        /// <param name="domElementSelector">A CSS selector that uniquely identifies a DOM element.</param>
        public Task AddComponentAsync<TComponent>(string domElementSelector)
            where TComponent: IComponent
        {
            return AddComponentAsync(typeof(TComponent), domElementSelector);
        }

        /// <summary>
        /// Associates the <see cref="IComponent"/> with the <see cref="BrowserRenderer"/>,
        /// causing it to be displayed in the specified DOM element.
        /// </summary>
        /// <param name="componentType">The type of the component.</param>
        /// <param name="domElementSelector">A CSS selector that uniquely identifies a DOM element.</param>
        public Task AddComponentAsync(Type componentType, string domElementSelector)
        {
            var component = InstantiateComponent(componentType);
            var componentId = AssignRootComponentId(component);

            var attachComponentTask = JSRuntime.Current.InvokeAsync<object>(
                "Blazor._internal.attachRootComponentToElement",
                RendererId,
                domElementSelector,
                componentId);
            CaptureAsyncExceptions(attachComponentTask);
            return RenderRootComponentAsync(componentId);
        }

        /// <inheritdoc />
        protected override Task UpdateDisplayAsync(in RenderBatch batch)
        {
            var bytes = MessagePackSerializer.Serialize(batch);
            var base64 = Convert.ToBase64String(bytes);
            ElectronNET.API.Electron.IpcMain.Send(_window, "JS.RenderBatch", RendererId, base64);

            // TODO: Consider finding a way to get back a completion message from the Electron side
            // in case there was an error. We don't really need to wait for anything to happen, since
            // this is not prerendering and we don't care how quickly the UI is updated, but it would
            // be desirable to flow back errors.
            return Task.CompletedTask;
        }

        private void CaptureAsyncExceptions(Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    UnhandledException?.Invoke(this, t.Exception);
                }
            });
        }

        protected override void HandleException(Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}
