// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Various extension methods for the <see cref="IWebHookReceiverConfig"/> interface.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookReceiverConfigExtensions
    {
        /// <summary>
        /// Gets the receiver configuration with the given <paramref name="name"/> and <paramref name="id"/>.
        /// </summary>
        /// <param name="config">The current <see cref="IWebHookReceiverConfig"/>.</param>
        /// <param name="name">The name of the configuration to obtain. Typically this is the name of the receiver, e.g. <c>github</c>.</param>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this <see cref="IWebHookReceiver"/>. This
        /// allows an <see cref="IWebHookReceiver"/> to support multiple WebHooks with individual configurations.</param>
        /// <param name="minLength">The minimum length of the key value.</param>
        /// <param name="maxLength">The maximum length of the key value.</param>
        public static async Task<string> GetReceiverConfigAsync(this IWebHookReceiverConfig config, string name, string id, int minLength, int maxLength)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            // Look up configuration for this receiver and instance
            string secret = await config.GetReceiverConfigAsync(name, id);

            // Verify that configuration value matches length requirements
            if (secret == null || secret.Length < minLength || secret.Length > maxLength)
            {
                return null;
            }
            return secret;
        }
    }
}
