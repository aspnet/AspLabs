// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookRegistrationsManager"/> for managing registrations for
    /// a given user.
    /// </summary>
    public class WebHookRegistrationsManager : IWebHookRegistrationsManager
    {
        private readonly IWebHookManager _manager;
        private readonly IWebHookStore _store;
        private readonly IWebHookFilterManager _filterManager;
        private readonly IWebHookUser _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookRegistrationsManager"/> class with default
        /// settings.
        /// </summary>
        /// <param name="manager">The current <see cref="IWebHookManager"/>.</param>
        /// <param name="store">The current <see cref="IWebHookStore"/>.</param>
        /// <param name="filterManager">The current <see cref="IWebHookFilterManager"/>.</param>
        /// <param name="userManager">The current <see cref="IWebHookUser"/>.</param>
        public WebHookRegistrationsManager(IWebHookManager manager, IWebHookStore store, IWebHookFilterManager filterManager, IWebHookUser userManager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            if (filterManager == null)
            {
                throw new ArgumentNullException(nameof(filterManager));
            }
            if (userManager == null)
            {
                throw new ArgumentNullException(nameof(userManager));
            }
            _manager = manager;
            _store = store;
            _filterManager = filterManager;
            _userManager = userManager;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<WebHook>> GetWebHooksAsync(IPrincipal user, Func<string, WebHook, Task> predicate)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            IEnumerable<WebHook> webHooks = await _store.GetAllWebHooksAsync(userId);
            await ApplyServiceSideFilterPredicate(userId, webHooks, predicate);
            return webHooks;
        }

        /// <inheritdoc />
        public virtual async Task<WebHook> LookupWebHookAsync(IPrincipal user, string id, Func<string, WebHook, Task> predicate)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var webHook = await _store.LookupWebHookAsync(userId, id);
            await ApplyServiceSideFilterPredicate(userId, new[] { webHook }, predicate);
            return webHook;
        }

        /// <inheritdoc />
        public virtual async Task<StoreResult> AddWebHookAsync(IPrincipal user, WebHook webHook, Func<string, WebHook, Task> predicate)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            var userId = await _userManager.GetUserIdAsync(user);
            await ApplyServiceSideFilterPredicate(userId, new[] { webHook }, predicate);

            var result = await _store.InsertWebHookAsync(userId, webHook);
            return result;
        }

        /// <inheritdoc />
        public virtual async Task<StoreResult> UpdateWebHookAsync(IPrincipal user, WebHook webHook, Func<string, WebHook, Task> predicate)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            var userId = await _userManager.GetUserIdAsync(user);
            await ApplyServiceSideFilterPredicate(userId, new[] { webHook }, predicate);

            var result = await _store.UpdateWebHookAsync(userId, webHook);
            return result;
        }

        /// <inheritdoc />
        public virtual async Task<StoreResult> DeleteWebHookAsync(IPrincipal user, string id)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var result = await _store.DeleteWebHookAsync(userId, id);
            return result;
        }

        /// <inheritdoc />
        public virtual async Task DeleteAllWebHooksAsync(IPrincipal user)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            await _store.DeleteAllWebHooksAsync(userId);
        }

        /// <summary>
        /// Verifies the <see cref="WebHook.Id"/> of the given <paramref name="webHook"/>
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify.</param>
        public virtual Task VerifyIdAsync(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            // Ensure we have a normalized ID for the WebHook
            webHook.Id = null;
            return Task.FromResult(true);
        }

        /// <summary>
        /// Verifies the <see cref="WebHook.Secret"/> of the given <paramref name="webHook"/>.
        /// If no secret is provided then create one here. This allows for scenarios
        /// where the caller may use a secret directly embedded in the WebHook URI, or
        /// has some other way of enforcing security.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify.</param>
        public virtual Task VerifySecretAsync(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            if (string.IsNullOrEmpty(webHook.Secret))
            {
                webHook.Secret = Guid.NewGuid().ToString("N");
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Verifies that the <see cref="WebHook.Filters"/> for the given <paramref name="webHook"/>
        /// only contain registered filters provided by the <see cref="IWebHookFilterManager"/>.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify.</param>
        public virtual async Task VerifyFiltersAsync(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            // If there are no filters then add our wildcard filter.
            if (webHook.Filters.Count == 0)
            {
                webHook.Filters.Add(WildcardWebHookFilterProvider.Name);
                return;
            }

            var filters = await _filterManager.GetAllWebHookFiltersAsync();
            var normalizedFilters = new HashSet<string>();
            var invalidFilters = new List<string>();
            foreach (var filter in webHook.Filters)
            {
                if (filters.TryGetValue(filter, out var hookFilter))
                {
                    normalizedFilters.Add(hookFilter.Name);
                }
                else
                {
                    invalidFilters.Add(filter);
                }
            }

            if (invalidFilters.Count > 0)
            {
                var invalidFiltersMessage = string.Join(", ", invalidFilters);
                var message = string.Format(CultureInfo.CurrentCulture, CustomResources.RegistrationsManager_InvalidFilters, invalidFiltersMessage);
                throw new InvalidOperationException(message);
            }
            else
            {
                webHook.Filters.Clear();
                foreach (var filter in normalizedFilters)
                {
                    webHook.Filters.Add(filter);
                }
            }
        }

        /// <summary>
        /// Verifies the <see cref="WebHook.WebHookUri"/> by issuing an HTTP GET request to the provided
        /// <paramref name="webHook"/> to ensure that it is reachable and expects WebHooks. The WebHook
        /// validation response is expected to echo the contents of the <c>echo</c> query parameter unless
        /// the WebHook URI has a <c>NoEcho</c> query parameter.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify.</param>
        public virtual async Task VerifyAddressAsync(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            await _manager.VerifyWebHookAsync(webHook);
        }

        private static async Task ApplyServiceSideFilterPredicate(string userId, IEnumerable<WebHook> webHooks, Func<string, WebHook, Task> predicate)
        {
            if (predicate != null)
            {
                foreach (var hook in webHooks)
                {
                    if (hook != null)
                    {
                        await predicate(userId, hook);
                    }
                }
            }
        }
    }
}
