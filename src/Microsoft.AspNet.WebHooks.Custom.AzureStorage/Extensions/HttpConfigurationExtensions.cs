// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Threading;
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

        private static IStorageManager _storageManager;

        /// <summary>
        /// Configures a Microsoft Azure Table Storage implementation of <see cref="IWebHookStore"/>
        /// which provides a persistent store for registered WebHooks used by the custom WebHooks module.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeCustomWebHooksAzureQueueSender(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            WebHooksConfig.Initialize(config);

            ILogger logger = config.DependencyResolver.GetLogger();
            SettingsDictionary settings = config.DependencyResolver.GetSettings();

            IStorageManager storageManager = GetStorageManager(logger);
            IWebHookSender sender = new AzureWebHookSender(storageManager, settings, logger);
            CustomServices.SetSender(sender);
        }

        /// <summary>
        /// Configures a Microsoft Azure Table Storage implementation of <see cref="IWebHookStore"/>
        /// which provides a persistent store for registered WebHooks used by the custom WebHooks module.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeCustomWebHooksAzureStorage(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            WebHooksConfig.Initialize(config);

            ILogger logger = config.DependencyResolver.GetLogger();
            SettingsDictionary settings = config.DependencyResolver.GetSettings();

            IDataProtectionProvider provider = GetDataProtectionProvider();
            IDataProtector protector = provider.CreateProtector(Purpose);

            IStorageManager storageManager = GetStorageManager(logger);
            IWebHookStore store = new AzureWebHookStore(storageManager, settings, protector, logger);
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

        internal static IStorageManager GetStorageManager(ILogger logger)
        {
            if (_storageManager != null)
            {
                return _storageManager;
            }

            IStorageManager instance = new StorageManager(logger);
            Interlocked.CompareExchange(ref _storageManager, instance, null);
            return _storageManager;
        }
    }
}