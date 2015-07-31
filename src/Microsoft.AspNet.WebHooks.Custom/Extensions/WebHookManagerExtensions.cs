// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Extension methods for <see cref="IWebHookManager"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookManagerExtensions
    {
        /// <summary>
        /// Gets an <see cref="IWebHookStore"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="manager">The <see cref="IWebHookManager"/> implementation.</param>
        /// <param name="user">The user for which to lookup and dispatch matching WebHooks.</param>
        /// <param name="actions">One or more actions describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAsync(this IWebHookManager manager, string user, IEnumerable<string> actions, object data)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            IDictionary<string, object> dataAsDictionary = data as IDictionary<string, object>;
            if (dataAsDictionary == null && data != null)
            {
                dataAsDictionary = new Dictionary<string, object>();
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(data);
                foreach (PropertyDescriptor prop in properties)
                {
                    object val = prop.GetValue(data);
                    dataAsDictionary.Add(prop.Name, val);
                }
            }

            return manager.NotifyAsync(user, actions, dataAsDictionary);
        }
    }
}
