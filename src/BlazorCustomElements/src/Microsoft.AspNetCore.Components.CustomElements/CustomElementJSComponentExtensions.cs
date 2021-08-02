// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components;

namespace Microsoft.AspNetCore.Components.Web
{
    public static class CustomElementJSComponentExtensions
    {
        /// <summary>
        /// Allows the specified component type to be used as a custom element.
        /// </summary>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <param name="configuration">The <see cref="IJSComponentConfiguration"/>.</param>
        /// <param name="customElementName">A unique name for the custom element. This must conform to custom element naming rules, so it must contain a dash character.</param>
        public static void RegisterAsCustomElement<TComponent>(this IJSComponentConfiguration configuration, string customElementName) where TComponent : IComponent
            => configuration.RegisterForJavaScript<TComponent>(customElementName, "registerBlazorCustomElement");
    }
}
