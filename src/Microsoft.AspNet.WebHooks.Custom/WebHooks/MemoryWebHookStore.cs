// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookStore"/> storing registered WebHooks in memory.
    /// </summary>
    /// <remarks>Actual deployments should replace this with a persistent store, for example provided by
    /// <c>Microsoft.AspNet.WebHooks.Custom.AzureStorage</c>.
    /// </remarks>
    public class MemoryWebHookStore : IWebHookStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebHook>> _store =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, WebHook>>();

        /// <inheritdoc />
        public Task<ICollection<WebHook>> GetAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user = Normalize(user);

            ConcurrentDictionary<string, WebHook> userHooks;
            ICollection<WebHook> result = _store.TryGetValue(user, out userHooks) ? userHooks.Values : new Collection<WebHook>();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<ICollection<WebHook>> QueryWebHooksAsync(string user, IEnumerable<string> actions)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }

            user = Normalize(user);

            ICollection<WebHook> matches = new List<WebHook>();
            ConcurrentDictionary<string, WebHook> userHooks;
            if (_store.TryGetValue(user, out userHooks))
            {
                foreach (WebHook webHook in userHooks.Values)
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
            }

            return Task.FromResult(matches);
        }

        /// <inheritdoc />
        public Task<WebHook> LookupWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            user = Normalize(user);

            WebHook result = null;
            ConcurrentDictionary<string, WebHook> userHooks;
            if (_store.TryGetValue(user, out userHooks))
            {
                id = Normalize(id);
                userHooks.TryGetValue(id, out result);
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<StoreResult> InsertWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            user = Normalize(user);

            ConcurrentDictionary<string, WebHook> userHooks = _store.GetOrAdd(user, key => new ConcurrentDictionary<string, WebHook>());

            string id = Normalize(webHook.Id);
            bool inserted = userHooks.TryAdd(id, webHook);
            StoreResult result = inserted ? StoreResult.Success : StoreResult.Conflict;
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<StoreResult> UpdateWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            user = Normalize(user);

            ConcurrentDictionary<string, WebHook> userHooks;
            StoreResult result = StoreResult.NotFound;
            if (_store.TryGetValue(user, out userHooks))
            {
                string id = Normalize(webHook.Id);

                WebHook current;
                if (userHooks.TryGetValue(id, out current))
                {
                    bool updated = userHooks.TryUpdate(id, webHook, current);
                    result = updated ? StoreResult.Success : StoreResult.Conflict;
                }
            }
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<StoreResult> DeleteWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            user = Normalize(user);

            bool deleted = false;
            ConcurrentDictionary<string, WebHook> userHooks;
            StoreResult result = StoreResult.NotFound;
            if (_store.TryGetValue(user, out userHooks))
            {
                id = Normalize(id);

                WebHook current;
                deleted = userHooks.TryRemove(id, out current);
                result = deleted ? StoreResult.Success : StoreResult.NotFound;
            }
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task DeleteAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user = Normalize(user);

            ConcurrentDictionary<string, WebHook> userHooks;
            if (_store.TryGetValue(user, out userHooks))
            {
                userHooks.Clear();
            }

            return Task.FromResult(true);
        }

        private static string Normalize(string value)
        {
            return value.ToLowerInvariant();
        }
    }
}
