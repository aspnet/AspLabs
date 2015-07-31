// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks.Diagnostics;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Extension methods for <see cref="IDependencyScope"/> facilitating getting the services used for receiving incoming WebHooks.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DependencyScopeExtensions
    {
        /// <summary>
        /// Gets an <see cref="IWebHookReceiverManager"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none is registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookReceiverManager"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookReceiverManager GetReceiverManager(this IDependencyScope services)
        {
            IWebHookReceiverManager receiverManager = services.GetService<IWebHookReceiverManager>();
            if (receiverManager == null)
            {
                IEnumerable<IWebHookReceiver> receivers = services.GetReceivers();
                ILogger logger = services.GetLogger();
                receiverManager = ReceiverServices.GetReceiverManager(receivers, logger);
            }
            return receiverManager;
        }

        /// <summary>
        /// Gets an <see cref="IWebHookHandlerSorter"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none is registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookReceiverManager"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookHandlerSorter GetHandlerSorter(this IDependencyScope services)
        {
            IWebHookHandlerSorter handlerSorter = services.GetService<IWebHookHandlerSorter>();
            if (handlerSorter == null)
            {
                handlerSorter = ReceiverServices.GetHandlerSorter();
            }
            return handlerSorter;
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookReceiver"/> instances registered with the Dependency Injection engine
        /// or an empty collection if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the registered instances.</returns>
        public static IEnumerable<IWebHookReceiver> GetReceivers(this IDependencyScope services)
        {
            IEnumerable<IWebHookReceiver> receivers = services.GetServices<IWebHookReceiver>();
            if (receivers == null || !receivers.Any())
            {
                receivers = ReceiverServices.GetReceivers();
            }
            return receivers;
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookHandler"/> instances registered with the Dependency Injection engine
        /// or an empty collection if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the registered instances.</returns>
        public static IEnumerable<IWebHookHandler> GetHandlers(this IDependencyScope services)
        {
            IEnumerable<IWebHookHandler> handlers = services.GetServices<IWebHookHandler>();
            if (handlers == null || !handlers.Any())
            {
                handlers = ReceiverServices.GetHandlers();
            }

            // Sort handlers
            IWebHookHandlerSorter sorter = services.GetHandlerSorter();
            return sorter.SortHandlers(handlers);
        }
    }
}
