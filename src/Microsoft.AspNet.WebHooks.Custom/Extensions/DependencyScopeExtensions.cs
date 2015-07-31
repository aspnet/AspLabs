// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Extension methods for <see cref="IDependencyScope"/> facilitating getting the services used by custom WebHooks.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DependencyScopeExtensions
    {
        /// <summary>
        /// Gets an <see cref="IWebHookStore"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookStore"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookStore GetStore(this IDependencyScope services)
        {
            IWebHookStore store = services.GetService<IWebHookStore>();
            return store ?? CustomServices.GetStore();
        }

        /// <summary>
        /// Gets an <see cref="IWebHookUser"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookUser"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookUser GetUser(this IDependencyScope services)
        {
            IWebHookUser userId = services.GetService<IWebHookUser>();
            return userId ?? CustomServices.GetUser();
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookFilterProvider"/> instances registered with the Dependency Injection engine
        /// or an empty collection if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the registered instances.</returns>
        public static IEnumerable<IWebHookFilterProvider> GetFilterProviders(this IDependencyScope services)
        {
            IEnumerable<IWebHookFilterProvider> filterProviders = services.GetServices<IWebHookFilterProvider>();
            if (filterProviders == null || !filterProviders.Any())
            {
                filterProviders = CustomServices.GetFilterProviders();
            }
            return filterProviders;
        }

        /// <summary>
        /// Gets an <see cref="IWebHookFilterManager"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookFilterManager"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookFilterManager GetFilterManager(this IDependencyScope services)
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
        /// Gets an <see cref="IWebHookManager"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookManager"/> instance or a default implementation if none are registered.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller.")]
        public static IWebHookManager GetManager(this IDependencyScope services)
        {
            IWebHookManager manager = services.GetService<IWebHookManager>();
            if (manager == null)
            {
                IWebHookStore store = services.GetStore();
                ILogger logger = services.GetLogger();
                manager = CustomServices.GetManager(store, logger);
            }
            return manager;
        }
    }
}
