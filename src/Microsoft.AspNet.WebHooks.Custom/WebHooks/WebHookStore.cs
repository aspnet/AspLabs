// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstract <see cref="IWebHookStore"/> implementation which can be used to base other implementations on. 
    /// </summary>
    public abstract class WebHookStore : IWebHookStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookStore"/> class.
        /// </summary>
        protected WebHookStore()
        {
        }

        /// <inheritdoc />
        public abstract Task<ICollection<WebHook>> GetAllWebHooksAsync(string user);

        /// <inheritdoc />
        public virtual async Task<ICollection<WebHook>> QueryWebHooksAsync(string user, IEnumerable<string> actions)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }

            ICollection<WebHook> webHooks = await GetAllWebHooksAsync(user);
            ICollection<WebHook> matches = new List<WebHook>();
            foreach (WebHook webHook in webHooks)
            {
                if (webHook.IsPaused)
                {
                    continue;
                }

                foreach (string action in actions)
                {
                    if (webHook.MatchesAction(action))
                    {
                        matches.Add(webHook);
                        break;
                    }
                }
            }

            return matches;
        }

        /// <inheritdoc />
        public abstract Task<WebHook> LookupWebHookAsync(string user, string id);

        /// <inheritdoc />
        public abstract Task<StoreResult> InsertWebHookAsync(string user, WebHook webHook);

        /// <inheritdoc />
        public abstract Task<StoreResult> UpdateWebHookAsync(string user, WebHook webHook);

        /// <inheritdoc />
        public abstract Task<StoreResult> DeleteWebHookAsync(string user, string id);

        /// <inheritdoc />
        public abstract Task DeleteAllWebHooksAsync(string user);

        /// <summary>
        /// Normalizes a given key to ensure consistent lookups.
        /// </summary>
        /// <param name="key">The key to normalize.</param>
        /// <returns>The normalized key.</returns>
        protected virtual string NormalizeKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return key.ToLowerInvariant();
        }
    }
}
