// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for manipulating the registration flow of WebHooks as it goes through the
    /// <see cref="Controllers.WebHookRegistrationsController"/> class. The <see cref="IWebHookRegistrar"/> allows
    /// an implementation to change, modify, or reject WebHook registrations as they are created or updated.
    /// This can for example be used to add filters to registrations enabling broadcast notifications
    /// or specific group notifications.
    /// </summary>
    public interface IWebHookRegistrar
    {
        /// <summary>
        /// This method is called as part of creating or updating a <see cref="WebHook"/> registration. 
        /// If an <see cref="Exception"/> is thrown, then the operation is rejected. As registrations 
        /// can be edited by the user, any filters added here must either be listed by an
        /// <see cref="IWebHookFilterManager"/> implementation, or prefixed by 
        /// <see cref="WebHookRegistrar.PrivateFilterPrefix"/> in order to remain hidden from the user. 
        /// Failure to do so will lead to WebHook registration updates being rejected due to unknown filters.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="webHook">The incoming <see cref="WebHook"/> to inspect, manipulate, or reject.</param>
        Task RegisterAsync(HttpRequestMessage request, WebHook webHook);
    }
}
