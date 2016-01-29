// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;

namespace Microsoft.AspNet.WebHooks.Services
{
    /// <summary>
    /// Provides singleton instances of common WebHook services such as a default
    /// <see cref="ILogger"/> implementation, <see cref="SettingsDictionary"/> etc.
    /// If alternative implementations are provided by a Dependency Injection engine then
    /// those instances are used instead.
    /// </summary>
    public static class CommonServices
    {
        private static ILogger _logger;
        private static SettingsDictionary _settings;

        /// <summary>
        /// Gets a default <see cref="ILogger"/> implementation which is used if none are registered with the
        /// Dependency Engine.
        /// </summary>
        public static ILogger GetLogger()
        {
            if (_logger == null)
            {
                ILogger instance = new TraceLogger();
                Interlocked.CompareExchange(ref _logger, instance, null);
            }
            return _logger;
        }

        /// <summary>
        /// Gets a default <see cref="SettingsDictionary"/> instance which is used if none are registered with the
        /// Dependency Engine.
        /// </summary>
        public static SettingsDictionary GetSettings()
        {
            if (_settings == null)
            {
                DefaultSettingsProvider settingsProvider = new DefaultSettingsProvider();
                SettingsDictionary instance = settingsProvider.GetSettings();
                Interlocked.CompareExchange(ref _settings, instance, null);
            }
            return _settings;
        }

        /// <summary>
        /// For testing purposes
        /// </summary>
        internal static void Reset()
        {
            _logger = null;
            _settings = null;
        }
    }
}
