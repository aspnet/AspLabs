// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstract base class implementation of <see cref="IWebHookRegistrar"/>. An <see cref="IWebHookRegistrar"/>
    /// implementation can be used to change, modify, or reject WebHook registrations as they are created or updated
    /// through the <see cref="Controllers.WebHookRegistrationsController"/>. This can for example be used to add 
    /// filters to WebHook registrations enabling broadcast notifications or specific group notifications.
    /// </summary>
    public abstract class WebHookRegistrar : IWebHookRegistrar
    {
        private const string Prefix = "MS_Private_";

        /// <summary>
        /// Gets a prefix indicating that a WebHook registration filter is private to the server implementation
        /// and should not be made visible to the user. As WebHook registrations can be added or edited by the user, 
        /// all registration filters must either be listed by an <see cref="IWebHookFilterManager"/> implementation, 
        /// or prefixed by <see cref="WebHookRegistrar.PrivateFilterPrefix"/> in order to remain hidden from the user. 
        /// Failure to do so will lead to WebHook registration updates being rejected due to unknown filters.
        /// </summary>
        public static string PrivateFilterPrefix
        {
            get
            {
                return Prefix;
            }
        }

        /// <inheritdoc />
        public abstract Task RegisterAsync(HttpRequestMessage request, WebHook webHook);
    }
}
