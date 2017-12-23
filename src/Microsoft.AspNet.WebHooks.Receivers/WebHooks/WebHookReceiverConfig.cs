// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides a default <see cref="IWebHookReceiverConfig"/> implementation which manages <see cref="IWebHookReceiver"/>
    /// configuration using application settings. The name of the application setting is '<c>MS_WebHookReceiverSecret_&lt;name&gt;</c>'
    /// where '<c>name</c>' is the name of the receiver, for example <c>github</c>. The value is a comma-separated list of secrets,
    /// using an ID to differentiate between them. For example, '<c>secret0, id1=secret1, id2=secret2</c>'.
    /// The corresponding WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/custom/{id}</c>'.
    /// </summary>
    public class WebHookReceiverConfig : IWebHookReceiverConfig
    {
        internal const string ConfigKeyPrefix = "MS_WebHookReceiverSecret_";

        private readonly IDictionary<string, string> _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookReceiverConfig"/> which will use the application
        /// settings set in the given <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The <see cref="SettingsDictionary"/> to use for reading <see cref="IWebHookReceiver"/> configuration.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public WebHookReceiverConfig(SettingsDictionary settings, ILogger logger)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _config = ReadSettings(settings, logger);
        }

        /// <inheritdoc />
        public virtual Task<string> GetReceiverConfigAsync(string name, string id)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (id == null)
            {
                id = string.Empty;
            }

            var key = GetConfigKey(name, id);
            var result = _config.TryGetValue(key, out var value) ? value : null;
            return Task.FromResult(result);
        }

        internal static IDictionary<string, string> ReadSettings(SettingsDictionary settings, ILogger logger)
        {
            IDictionary<string, string> config = new Dictionary<string, string>();
            foreach (var setting in settings)
            {
                var key = setting.Key;
                if (key.Length > ConfigKeyPrefix.Length && key.StartsWith(ConfigKeyPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    // Extract receiver name
                    var receiver = key.Substring(ConfigKeyPrefix.Length);

                    // Parse values
                    var segments = setting.Value.SplitAndTrim(',');
                    foreach (var segment in segments)
                    {
                        var values = segment.SplitAndTrim('=');
                        if (values.Length == 1)
                        {
                            AddKey(config, logger, receiver, string.Empty, values[0]);
                        }
                        else if (values.Length == 2)
                        {
                            AddKey(config, logger, receiver, values[0], values[1]);
                        }
                        else
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Config_BadValue, key);
                            logger.Error(message);
                            throw new InvalidOperationException(message);
                        }
                    }
                }
            }

            if (config.Count == 0)
            {
                var format = ConfigKeyPrefix + "<receiver>";
                var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Config_NoConfig, format);
                logger.Error(message);
            }

            return config;
        }

        internal static void AddKey(IDictionary<string, string> config, ILogger logger, string receiver, string id, string value)
        {
            var lookupKey = GetConfigKey(receiver, id);

            try
            {
                config.Add(lookupKey, value);
                var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Config_AddedName, receiver, id);
                logger.Info(message);
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Config_AddFailure, receiver, id, ex.Message);
                logger.Error(message, ex);
                throw new InvalidOperationException(message);
            }
        }

        internal static string GetConfigKey(string receiver, string id)
        {
            return (receiver + "/" + id).ToLowerInvariant();
        }
    }
}
