// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;

namespace System.Web.Mvc
{
    /// <summary>
    /// Extension methods for <see cref="IDependencyResolver"/> facilitating getting the services used by custom WebHooks.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DependencyResolverExtensions
    {
        /// <summary>
        /// Gets an <see cref="ILogger"/> implementation registered with the Dependency Injection engine
        /// or a default <see cref="System.Diagnostics.Trace"/> implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyResolver"/> implementation.</param>
        /// <returns>The registered <see cref="ILogger"/> instance or a default implementation if none are registered.</returns>
        public static ILogger GetLogger(this IDependencyResolver services)
        {
            ILogger logger = services.GetService<ILogger>();
            return logger ?? CommonServices.GetLogger();
        }

        /// <summary>
        /// Gets a <see cref="SettingsDictionary"/> instance registered with the Dependency Injection engine
        /// or a default implementation based on application settings if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyResolver"/> implementation.</param>
        /// <returns>The registered <see cref="SettingsDictionary"/> instance or a default implementation if none are registered.</returns>
        public static SettingsDictionary GetSettings(this IDependencyResolver services)
        {
            SettingsDictionary settings = services.GetService<SettingsDictionary>();
            return settings != null && settings.Count > 0 ? settings : CommonServices.GetSettings();
        }

        /// <summary>
        /// Gets an <see cref="IWebHookFilterManager"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyResolver"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookFilterManager"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookFilterManager GetFilterManager(this IDependencyResolver services)
        {
            IWebHookFilterManager filterManager = services.GetService<IWebHookFilterManager>();
            if (filterManager == null)
            {
                IEnumerable<IWebHookFilterProvider> filterProviders = services.GetFilterProviders();
                filterManager = CustomServices.GetFilterManager(filterProviders);
            }
            return filterManager;
        }

        /// <summary>
        /// Gets an <see cref="IWebHookSender"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyResolver"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookSender"/> instance or a default implementation if none are registered.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller.")]
        public static IWebHookSender GetSender(this IDependencyResolver services)
        {
            IWebHookSender sender = services.GetService<IWebHookSender>();
            if (sender == null)
            {
                ILogger logger = services.GetLogger();
                sender = CustomServices.GetSender(logger);
            }
            return sender;
        }

        /// <summary>
        /// Gets an <see cref="IWebHookManager"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyResolver"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookManager"/> instance or a default implementation if none are registered.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller.")]
        public static IWebHookManager GetManager(this IDependencyResolver services)
        {
            IWebHookManager manager = services.GetService<IWebHookManager>();
            if (manager == null)
            {
                IWebHookStore store = services.GetStore();
                IWebHookSender sender = services.GetSender();
                ILogger logger = services.GetLogger();
                manager = CustomServices.GetManager(store, sender, logger);
            }
            return manager;
        }

        /// <summary>
        /// Gets an <see cref="IWebHookStore"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyResolver"/> implementation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the registered instances.</returns>
        public static IWebHookStore GetStore(this IDependencyResolver services)
        {
            IWebHookStore store = services.GetService<IWebHookStore>();
            return store ?? CustomServices.GetStore();
        }

        /// <summary>
        /// Gets an <see cref="IWebHookUser"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyResolver"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookUser"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookUser GetUser(this IDependencyResolver services)
        {
            IWebHookUser user = services.GetService<IWebHookUser>();
            return user ?? CustomServices.GetUser();
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookFilterProvider"/> instances registered with the Dependency Injection engine
        /// or an empty collection if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyResolver"/> implementation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the registered instances.</returns>
        public static IEnumerable<IWebHookFilterProvider> GetFilterProviders(this IDependencyResolver services)
        {
            IEnumerable<IWebHookFilterProvider> filterProviders = services.GetServices<IWebHookFilterProvider>();
            if (filterProviders == null || !filterProviders.Any())
            {
                filterProviders = CustomServices.GetFilterProviders();
            }
            return filterProviders;
        }
    }
}
