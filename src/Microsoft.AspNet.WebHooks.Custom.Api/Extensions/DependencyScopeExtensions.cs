// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks.Services;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Extension methods for <see cref="IDependencyScope"/> facilitating getting the services used by custom WebHooks APIs.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DependencyScopeExtensions
    {
        /// <summary>
        /// Gets an <see cref="IWebHookIdValidator"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookIdValidator"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookIdValidator GetIdValidator(this IDependencyScope services)
        {
            IWebHookIdValidator validator = services.GetService<IWebHookIdValidator>();
            return validator ?? CustomApiServices.GetIdValidator();
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookRegistrar"/> instances registered with the Dependency Injection engine
        /// or an empty collection if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IDependencyScope"/> implementation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the registered instances.</returns>
        public static IEnumerable<IWebHookRegistrar> GetRegistrars(this IDependencyScope services)
        {
            IEnumerable<IWebHookRegistrar> registrar = services.GetServices<IWebHookRegistrar>();
            if (registrar == null || !registrar.Any())
            {
                registrar = CustomApiServices.GetRegistrars();
            }
            return registrar;
        }
    }
}
