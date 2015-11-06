// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Data.Entity;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.Framework.DependencyInjection;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        private const string ApplicationName = "Microsoft.AspNet.WebHooks";
        private const string Purpose = "WebHookPersistence";
        private const string DataProtectionKeysFolderName = "DataProtection-Keys";

        /// <summary>
        /// Configures a Microsoft SQL Server Storage implementation of <see cref="IWebHookStore"/>
        /// which provides a persistent store for registered WebHooks used by the custom WebHooks module.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeCustomWebHooksSqlStorage(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            WebHooksConfig.Initialize(config);

            ILogger logger = config.DependencyResolver.GetLogger();
            SettingsDictionary settings = config.DependencyResolver.GetSettings();

            // We explicitly set the DB initializer to null to avoid that an existing DB is initialized wrongly.
            Database.SetInitializer<WebHookStoreContext>(null);

            IDataProtectionProvider provider = GetDataProtectionProvider();
            IDataProtector protector = provider.CreateProtector(Purpose);

            IWebHookStore store = new SqlWebHookStore(settings, protector, logger);
            CustomServices.SetStore(store);
        }

        /// <summary>
        /// This follows the same initialization that is provided when <see cref="IDataProtectionProvider"/>
        /// is initialized within ASP.NET 5.0 Dependency Injection.
        /// </summary>
        /// <returns>A fully initialized <see cref="IDataProtectionProvider"/>.</returns>
        internal static IDataProtectionProvider GetDataProtectionProvider()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection();
            IServiceProvider services = serviceCollection.BuildServiceProvider();
            return services.GetDataProtectionProvider();
        }
    }
}
