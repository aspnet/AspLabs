// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System;
using System.Globalization;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.AspNet.WebHooks.Storage;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookStore"/> storing registered WebHooks in Microsoft SQL Server.
    /// </summary>
    [CLSCompliant(false)]
    public class SqlWebHookStore : DbWebHookStore<WebHookStoreContext, Registration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlWebHookStore"/> class with the given <paramref name="settings"/>,
        /// <paramref name="protector"/>, and <paramref name="logger"/>.
        /// </summary>
        public SqlWebHookStore(SettingsDictionary settings, IDataProtector protector, ILogger logger)
            : base(protector, logger)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            CheckSqlStorageConnectionString(settings);
        }

        /// <summary>
        /// Provides a static method for creating a standalone <see cref="SqlWebHookStore"/> instance.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use.</param>
        /// <returns>An initialized <see cref="SqlWebHookStore"/> instance.</returns>
        public static IWebHookStore CreateStore(ILogger logger)
        {
            SettingsDictionary settings = CommonServices.GetSettings();
            IDataProtector protector = DataSecurity.GetDataProtector();
            IWebHookStore store = new SqlWebHookStore(settings, protector, logger);
            return store;
        }

        internal static string CheckSqlStorageConnectionString(SettingsDictionary settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ConnectionSettings connection;
            if (!settings.Connections.TryGetValue(WebHookStoreContext.ConnectionStringName, out connection) || connection == null || string.IsNullOrEmpty(connection.ConnectionString))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_NoConnectionString, WebHookStoreContext.ConnectionStringName);
                throw new InvalidOperationException(msg);
            }
            return connection.ConnectionString;
        }
    }
}
