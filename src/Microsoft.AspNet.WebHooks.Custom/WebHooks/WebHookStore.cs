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
        public abstract Task<ICollection<WebHook>> QueryWebHooksAsync(string user, IEnumerable<string> actions, Func<WebHook, string, bool> predicate);

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

        /// <inheritdoc />
        public abstract Task<ICollection<WebHook>> QueryWebHooksAcrossAllUsersAsync(IEnumerable<string> actions, Func<WebHook, string, bool> predicate);

        /// <summary>
        /// Normalizes a given key to ensure consistent lookups.
        /// </summary>
        /// <param name="key">The key to normalize.</param>
        /// <returns>The normalized key.</returns>
        protected virtual string NormalizeKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return key.ToLowerInvariant();
        }

        /// <summary>
        /// Checks that the given <paramref name="webHook"/> is not paused and matches at least
        /// one of the given <paramref name="actions"/>.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> instance to operate on.</param>
        /// <param name="actions">The set of actions to match against the <paramref name="webHook"/> filters.</param>
        /// <returns><c>true</c> if the given <paramref name="webHook"/> matches one of the pro</returns>
        protected virtual bool MatchesAnyAction(WebHook webHook, IEnumerable<string> actions)
        {
            return webHook != null && !webHook.IsPaused && webHook.MatchesAnyAction(actions);
        }
    }
}
