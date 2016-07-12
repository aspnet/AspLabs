// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookStore"/> storing registered WebHooks in memory.
    /// </summary>
    /// <remarks>Actual deployments should replace this with a persistent store, for example provided by
    /// <c>Microsoft.AspNet.WebHooks.Custom.AzureStorage</c>.
    /// </remarks>
    public class MemoryWebHookStore : WebHookStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebHook>> _store =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, WebHook>>();

        /// <inheritdoc />
        public override Task<ICollection<WebHook>> GetAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            ConcurrentDictionary<string, WebHook> userHooks;
            ICollection<WebHook> result = _store.TryGetValue(user, out userHooks) ? userHooks.Values : new Collection<WebHook>();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public override Task<ICollection<WebHook>> QueryWebHooksAsync(string user, IEnumerable<string> actions, Func<WebHook, string, bool> predicate)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            user = NormalizeKey(user);

            predicate = predicate ?? DefaultPredicate;

            ConcurrentDictionary<string, WebHook> userHooks;
            if (_store.TryGetValue(user, out userHooks))
            {
                ICollection<WebHook> matches = userHooks.Values.Where(w => MatchesAnyAction(w, actions) && predicate(w, user))
                    .ToArray();
                return Task.FromResult(matches);
            }

            return Task.FromResult<ICollection<WebHook>>(new WebHook[0]);
        }

        /// <inheritdoc />
        public override Task<WebHook> LookupWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            user = NormalizeKey(user);

            WebHook result = null;
            ConcurrentDictionary<string, WebHook> userHooks;
            if (_store.TryGetValue(user, out userHooks))
            {
                id = NormalizeKey(id);
                userHooks.TryGetValue(id, out result);
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public override Task<StoreResult> InsertWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            user = NormalizeKey(user);

            ConcurrentDictionary<string, WebHook> userHooks = _store.GetOrAdd(user, key => new ConcurrentDictionary<string, WebHook>());

            string id = NormalizeKey(webHook.Id);
            bool inserted = userHooks.TryAdd(id, webHook);
            StoreResult result = inserted ? StoreResult.Success : StoreResult.Conflict;
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public override Task<StoreResult> UpdateWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            user = NormalizeKey(user);

            ConcurrentDictionary<string, WebHook> userHooks;
            StoreResult result = StoreResult.NotFound;
            if (_store.TryGetValue(user, out userHooks))
            {
                string id = NormalizeKey(webHook.Id);

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
        public override Task<StoreResult> DeleteWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            user = NormalizeKey(user);

            ConcurrentDictionary<string, WebHook> userHooks;
            StoreResult result = StoreResult.NotFound;
            if (_store.TryGetValue(user, out userHooks))
            {
                id = NormalizeKey(id);

                WebHook current;
                bool deleted = userHooks.TryRemove(id, out current);
                result = deleted ? StoreResult.Success : StoreResult.NotFound;
            }
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public override Task DeleteAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            ConcurrentDictionary<string, WebHook> userHooks;
            if (_store.TryGetValue(user, out userHooks))
            {
                userHooks.Clear();
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public override Task<ICollection<WebHook>> QueryWebHooksAcrossAllUsersAsync(IEnumerable<string> actions, Func<WebHook, string, bool> predicate)
        {
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            predicate = predicate ?? DefaultPredicate;

            var matches = new List<WebHook>();
            foreach (var user in _store)
            {
                matches.AddRange(user.Value.Where(w => MatchesAnyAction(w.Value, actions) && predicate(w.Value, user.Key)).Select(w => w.Value));
            }
            return Task.FromResult<ICollection<WebHook>>(matches);
        }

        private static bool DefaultPredicate(WebHook webHook, string user)
        {
            return true;
        }
    }
}
