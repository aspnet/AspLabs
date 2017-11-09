// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Data.Entity;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.AspNetCore.DataProtection;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Configures a Microsoft SQL Server Storage implementation of <see cref="IWebHookStore"/>
        /// which provides a persistent store for registered WebHooks used by the custom WebHooks module.
        /// Using this initializer, the data will be encrypted using <see cref="IDataProtector"/>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeCustomWebHooksSqlStorage(this HttpConfiguration config)
        {
            InitializeCustomWebHooksSqlStorage(config, encryptData: true);
        }

        /// <summary>
        /// Configures a Microsoft SQL Server Storage implementation of <see cref="IWebHookStore"/>
        /// which provides a persistent store for registered WebHooks used by the custom WebHooks module.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        /// <param name="encryptData">Indicates whether the data should be encrypted using <see cref="IDataProtector"/> while persisted.</param>
        public static void InitializeCustomWebHooksSqlStorage(this HttpConfiguration config, bool encryptData)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            WebHooksConfig.Initialize(config);

            ILogger logger = config.DependencyResolver.GetLogger();
            SettingsDictionary settings = config.DependencyResolver.GetSettings();

            // We explicitly set the DB initializer to null to avoid that an existing DB is initialized wrongly.
            Database.SetInitializer<WebHookStoreContext>(null);

            IWebHookStore store;
            if (encryptData)
            {
                IDataProtector protector = DataSecurity.GetDataProtector();
                store = new SqlWebHookStore(settings, protector, logger);
            }
            else
            {
                store = new SqlWebHookStore(settings, logger);
            }
            CustomServices.SetStore(store);
        }
    }
}
