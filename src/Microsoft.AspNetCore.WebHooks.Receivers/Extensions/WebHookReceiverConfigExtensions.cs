// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Various extension methods for the <see cref="IWebHookReceiverConfig"/> interface.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookReceiverConfigExtensions
    {
        /// <summary>
        /// Gets the receiver configuration with the given <paramref name="configurationName"/> and
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="config">The current <see cref="IWebHookReceiverConfig"/>.</param>
        /// <param name="configurationName">
        /// The name of the configuration to obtain. Typically this is the name of the receiver, e.g. <c>github</c>.
        /// </param>
        /// <param name="id">
        /// A (potentially empty) ID of a particular configuration for this <see cref="IWebHookReceiver"/>. This
        /// allows an <see cref="IWebHookReceiver"/> to support multiple WebHook endpoints with individual
        /// configurations.
        /// </param>
        /// <param name="minLength">The minimum length of the key value.</param>
        /// <param name="maxLength">The maximum length of the key value.</param>
        public static async Task<string> GetReceiverConfigAsync(
            this IWebHookReceiverConfig config,
            string configurationName,
            string id,
            int minLength,
            int maxLength)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (configurationName == null)
            {
                throw new ArgumentNullException(nameof(configurationName));
            }

            // Look up configuration for this name and id.
            var secret = await config.GetReceiverConfigAsync(configurationName, id);

            // Verify that configuration value matches length requirements
            if (secret == null || secret.Length < minLength || secret.Length > maxLength)
            {
                return null;
            }

            return secret;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the configuration value with given <paramref name="key"/> is set to
        /// 'true'; otherwise <see langword="false"/>.
        /// </summary>
        /// <param name="config">The current <see cref="IWebHookReceiverConfig"/>.</param>
        /// <param name="key">The key to evaluate the value for.</param>
        /// <returns><see langword="true"/> if the value is set to 'true'; otherwise <see langword="false"/>.</returns>
        public static bool IsTrue(this IWebHookReceiverConfig config, string key)
        {
            var value = config.Configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return bool.TryParse(value.Trim(), out var isSet) ? isSet : false;
        }
    }
}
