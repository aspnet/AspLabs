// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.AspNet.WebHooks.Storage;
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
        /// Configures a Microsoft Azure Table Storage implementation of <see cref="IWebHookStore"/>
        /// which provides a persistent store for registered WebHooks used by the custom WebHooks module.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeCustomWebHooksAzureQueueSender(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            WebHooksConfig.Initialize(config);

            ILogger logger = config.DependencyResolver.GetLogger();
            SettingsDictionary settings = config.DependencyResolver.GetSettings();

            IStorageManager storageManager = StorageManager.GetInstance(logger);
            IWebHookSender sender = new AzureWebHookSender(storageManager, settings, logger);
            CustomServices.SetSender(sender);
        }

        /// <summary>
        /// Configures a Microsoft Azure Table Storage implementation of <see cref="IWebHookStore"/>
        /// which provides a persistent store for registered WebHooks used by the custom WebHooks module.
        /// Using this initializer, the data will be encrypted using <see cref="IDataProtector"/>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeCustomWebHooksAzureStorage(this HttpConfiguration config)
        {
            InitializeCustomWebHooksAzureStorage(config, encryptData: true);
        }

        /// <summary>
        /// Configures a Microsoft Azure Table Storage implementation of <see cref="IWebHookStore"/>
        /// which provides a persistent store for registered WebHooks used by the custom WebHooks module.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        /// <param name="encryptData">Indicates whether the data should be encrypted using <see cref="IDataProtector"/> while persisted.</param>
        public static void InitializeCustomWebHooksAzureStorage(this HttpConfiguration config, bool encryptData)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            WebHooksConfig.Initialize(config);

            ILogger logger = config.DependencyResolver.GetLogger();
            SettingsDictionary settings = config.DependencyResolver.GetSettings();

            IStorageManager storageManager = StorageManager.GetInstance(logger);
            IWebHookStore store;
            if (encryptData)
            {
                IDataProtector protector = DataSecurity.GetDataProtector();
                store = new AzureWebHookStore(storageManager, settings, protector, logger);
            }
            else
            {
                store = new AzureWebHookStore(storageManager, settings, logger);
            }
            CustomServices.SetStore(store);
        }
    }
}