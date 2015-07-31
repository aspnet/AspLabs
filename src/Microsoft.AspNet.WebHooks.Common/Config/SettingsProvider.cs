// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Microsoft.AspNet.WebHooks.Config
{
    /// <summary>
    /// Provides a default <see cref="SettingsDictionary"/> based on application settings from the global 
    /// <see cref="ConfigurationManager"/>.
    /// </summary>
    public class SettingsProvider
    {
        private readonly Lazy<SettingsDictionary> _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsProvider"/> class.
        /// </summary>
        public SettingsProvider()
        {
            _settings = new Lazy<SettingsDictionary>(() => InitializeSettings());
        }

        /// <summary>
        /// Initializes a <see cref="SettingsDictionary"/> instance using application settings.
        /// </summary>
        /// <returns>A fully initialized <see cref="SettingsDictionary"/>.</returns>
        public SettingsDictionary GetSettings()
        {
            return _settings.Value;
        }

        /// <summary>
        /// Initializes the <see cref="SettingsDictionary"/> provided in response to <see cref="M:GetSettings"/>.
        /// </summary>
        /// <returns>A fully initialized <see cref="SettingsDictionary"/>.</returns>
        protected virtual SettingsDictionary InitializeSettings()
        {
            SettingsDictionary settingsDictionary = new SettingsDictionary();

            NameValueCollection appSettingPairs = GetAppSettings();
            foreach (string key in appSettingPairs.AllKeys)
            {
                settingsDictionary[key] = appSettingPairs[key];
            }

            ConnectionStringSettingsCollection connectionSettings = ConfigurationManager.ConnectionStrings;
            foreach (ConnectionStringSettings connectionSetting in connectionSettings)
            {
                settingsDictionary.Connections.Add(
                    connectionSetting.Name,
                    new ConnectionSettings(connectionSetting.Name, connectionSetting.ConnectionString)
                    {
                        Provider = connectionSetting.ProviderName
                    });
            }

            return settingsDictionary;
        }

        /// <summary>
        /// Gets the current application settings in a mock-able manner.
        /// </summary>
        /// <returns>A <see cref="NameValueCollection"/> containing the application settings.</returns>
        protected virtual NameValueCollection GetAppSettings()
        {
            return ConfigurationManager.AppSettings;
        }
    }
}
