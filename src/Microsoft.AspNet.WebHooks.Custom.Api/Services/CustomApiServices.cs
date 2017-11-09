// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Utilities;

namespace Microsoft.AspNet.WebHooks.Services
{
    /// <summary>
    /// Provides singleton instances of custom WebHook API services used by this module.
    /// If alternative implementations are provided by a Dependency Injection engine then
    /// those instances are used instead.
    /// </summary>
    public static class CustomApiServices
    {
        private static IWebHookIdValidator _idValidator;
        private static IEnumerable<IWebHookRegistrar> _registrars;

        /// <summary>
        /// Gets a default <see cref="IWebHookIdValidator"/> implementation which is used if none are registered with the 
        /// Dependency Injection engine.
        /// </summary>
        /// <returns>A default <see cref="IWebHookIdValidator"/> instance.</returns>
        public static IWebHookIdValidator GetIdValidator()
        {
            if (_idValidator != null)
            {
                return _idValidator;
            }

            IWebHookIdValidator instance = new DefaultWebHookIdValidator();
            Interlocked.CompareExchange(ref _idValidator, instance, null);
            return _idValidator;
        }

        /// <summary>
        /// Sets a default <see cref="IWebHookIdValidator"/> implementation which is used if none are registered with the 
        /// Dependency Injection engine.
        /// </summary>
        /// <param name="instance">The <see cref="IWebHookIdValidator"/> to use. If <c>null</c> then a default implementation is used.</param>
        public static void SetIdValidator(IWebHookIdValidator instance)
        {
            _idValidator = instance;
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookRegistrar"/> instances discovered by a default 
        /// discovery mechanism which is used if none are registered with the Dependency Injection engine.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the discovered instances.</returns>
        public static IEnumerable<IWebHookRegistrar> GetRegistrars()
        {
            if (_registrars != null)
            {
                return _registrars;
            }

            IAssembliesResolver assembliesResolver = WebHooksConfig.Config.Services.GetAssembliesResolver();
            ICollection<Assembly> assemblies = assembliesResolver.GetAssemblies();
            IEnumerable<IWebHookRegistrar> instances = TypeUtilities.GetInstances<IWebHookRegistrar>(assemblies, t => TypeUtilities.IsType<IWebHookRegistrar>(t));
            Interlocked.CompareExchange(ref _registrars, instances, null);
            return _registrars;
        }

        /// <summary>
        /// For testing purposes
        /// </summary>
        internal static void Reset()
        {
            _registrars = null;
            _idValidator = null;
        }
    }
}
