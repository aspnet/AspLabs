// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Utilities;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides singleton instances of WebHook receiver services.
    /// If alternative implementations are provided by a Dependency Injection engine then
    /// those instances are used instead.
    /// </summary>
    public static class ReceiverServices
    {
        private static IWebHookReceiverManager _receiverManager;
        private static IWebHookHandlerSorter _handlerSorter;
        private static IEnumerable<IWebHookReceiver> _receivers;
        private static IEnumerable<IWebHookHandler> _handlers;

        /// <summary>
        /// Gets a default <see cref="IWebHookReceiverManager"/> implementation which is used if none are registered with the
        /// Dependency Injection engine.
        /// </summary>
        /// <returns>A default <see cref="IWebHookReceiverManager"/> instance.</returns>
        public static IWebHookReceiverManager GetReceiverManager(IEnumerable<IWebHookReceiver> receivers, ILogger logger)
        {
            if (_receiverManager != null)
            {
                return _receiverManager;
            }

            if (receivers == null)
            {
                throw new ArgumentNullException("receivers");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            IWebHookReceiverManager instance = new WebHookReceiverManager(receivers, logger);
            Interlocked.CompareExchange(ref _receiverManager, instance, null);
            return _receiverManager;
        }

        /// <summary>
        /// Gets a default <see cref="IWebHookHandlerSorter"/> implementation which is used if none are registered with the
        /// Dependency Injection engine.
        /// </summary>
        /// <returns>A default <see cref="IWebHookHandlerSorter"/> instance.</returns>
        public static IWebHookHandlerSorter GetHandlerSorter()
        {
            if (_handlerSorter != null)
            {
                return _handlerSorter;
            }

            IWebHookHandlerSorter instance = new WebHookHandlerSorter();
            Interlocked.CompareExchange(ref _handlerSorter, instance, null);
            return _handlerSorter;
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookReceiver"/> instances discovered by a default 
        /// discovery mechanism which is used if none are registered with the Dependency Injection engine.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the discovered instances.</returns>
        public static IEnumerable<IWebHookReceiver> GetReceivers()
        {
            if (_receivers != null)
            {
                return _receivers;
            }

            IAssembliesResolver assembliesResolver = WebHooksConfig.Config.Services.GetAssembliesResolver();
            ICollection<Assembly> assemblies = assembliesResolver.GetAssemblies();
            IEnumerable<IWebHookReceiver> instances = TypeUtilities.GetInstances<IWebHookReceiver>(assemblies, t => TypeUtilities.IsType<IWebHookReceiver>(t));
            Interlocked.CompareExchange(ref _receivers, instances, null);
            return _receivers;
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookHandler"/> instances discovered by a default 
        /// discovery mechanism which is used if none are registered with the Dependency Injection engine.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the discovered instances.</returns>
        public static IEnumerable<IWebHookHandler> GetHandlers()
        {
            if (_handlers != null)
            {
                return _handlers;
            }

            IAssembliesResolver assembliesResolver = WebHooksConfig.Config.Services.GetAssembliesResolver();
            ICollection<Assembly> assemblies = assembliesResolver.GetAssemblies();
            IEnumerable<IWebHookHandler> instances = TypeUtilities.GetInstances<IWebHookHandler>(assemblies, t => TypeUtilities.IsType<IWebHookHandler>(t));
            Interlocked.CompareExchange(ref _handlers, instances, null);
            return _handlers;
        }

        /// <summary>
        /// For testing purposes
        /// </summary>
        internal static void Reset()
        {
            _receiverManager = null;
            _handlerSorter = null;
            _receivers = null;
            _handlers = null;
        }
    }
}
