// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for managing <see cref="WebHook"/> registrations for a given user.
    /// The abstraction makes it easy to provide your own mechanism for users to register for WebHook notifications. 
    /// In addition to managing <see cref="WebHook"/> registrations on behalf of a user, the caller
    /// can through a set of predicates manage server-side filters that are not visible to the user and not 
    /// governed by the <see cref="IWebHookFilterManager"/>. This enables the caller to support 
    /// broadcast notifications or group notifications that are not directly exposed to users.
    /// 
    /// </summary>
    public interface IWebHookRegistrationsManager
    {
        /// <summary>
        /// Gets all registered <see cref="WebHook"/> instances for a given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user for which to get the registered <see cref="WebHook"/> instances.</param>
        /// <param name="predicate">An optional predicate which can be used to filter out server-side filters
        /// that are not governed by the set of registered filters provided by <see cref="IWebHookFilterManager"/>.</param>
        /// <returns>All registered <see cref="WebHook"/> instances for this user.</returns>
        Task<IEnumerable<WebHook>> GetWebHooksAsync(IPrincipal user, Func<string, WebHook, Task> predicate);

        /// <summary>
        /// Looks up an existing <see cref="WebHook"/> for a given <paramref name="user"/>. If a <see cref="WebHook"/>
        /// with this <paramref name="id"/> is not present then <c>null</c> is returned.
        /// </summary>
        /// <param name="user">The user for which to lookup a registered <see cref="WebHook"/> instance.</param>
        /// <param name="id">The ID uniquely identifying the WebHook.</param>
        /// <param name="predicate">An optional predicate which can be used to filter out server-side filters
        /// that are not governed by the set of registered filters provided by <see cref="IWebHookFilterManager"/>.</param>
        /// <returns>The <see cref="WebHook"/> instance or <c>null</c>.</returns>
        Task<WebHook> LookupWebHookAsync(IPrincipal user, string id, Func<string, WebHook, Task> predicate);

        /// <summary>
        /// Registers a new <see cref="WebHook"/> for a given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user for which to register the <see cref="WebHook"/>.</param>
        /// <param name="webHook">The <see cref="WebHook"/> to register.</param>
        /// <param name="predicate">An optional predicate which can be used to add server-side filters
        /// that are not governed by the set of registered filters provided by <see cref="IWebHookFilterManager"/>.</param>
        /// <returns>A <see cref="StoreResult"/> indicating the result of the operation.</returns>
        Task<StoreResult> AddWebHookAsync(IPrincipal user, WebHook webHook, Func<string, WebHook, Task> predicate);

        /// <summary>
        /// Updates an existing <see cref="WebHook"/> for a given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user for which to update the <see cref="WebHook"/>.</param>
        /// <param name="webHook">The <see cref="WebHook"/> to update.</param>
        /// <param name="predicate">An optional predicate which can be used to add server-side filters
        /// that are not governed by the set of registered filters provided by <see cref="IWebHookFilterManager"/>.</param>
        /// <returns>A <see cref="StoreResult"/> indicating the result of the operation.</returns>
        Task<StoreResult> UpdateWebHookAsync(IPrincipal user, WebHook webHook, Func<string, WebHook, Task> predicate);

        /// <summary>
        /// Deletes a registered <see cref="WebHook"/> for a given <paramref name="user"/>. If a <see cref="WebHook"/> 
        /// with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="user">The user for which to delete the <see cref="WebHook"/>.</param>
        /// <param name="id">The ID uniquely identifying the WebHook.</param>
        /// <returns>A <see cref="StoreResult"/> indicating the result of the operation.</returns>
        Task<StoreResult> DeleteWebHookAsync(IPrincipal user, string id);

        /// <summary>
        /// Deletes all existing <see cref="WebHook"/> instances for a given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user for which to delete all <see cref="WebHook"/> instances.</param>
        Task DeleteAllWebHooksAsync(IPrincipal user);

        /// <summary>
        /// Verifies the <see cref="WebHook.Id"/> of the given <paramref name="webHook"/>
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify.</param>
        Task VerifyIdAsync(WebHook webHook);

        /// <summary>
        /// Verifies the <see cref="WebHook.Secret"/> of the given <paramref name="webHook"/>.
        /// If no secret is provided then create one here. This allows for scenarios
        /// where the caller may use a secret directly embedded in the WebHook URI, or
        /// has some other way of enforcing security.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify.</param>
        Task VerifySecretAsync(WebHook webHook);

        /// <summary>
        /// Verifies that the <see cref="WebHook.Filters"/> for the given <paramref name="webHook"/> 
        /// only contain registered filters provided by the <see cref="IWebHookFilterManager"/>.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify.</param>
        Task VerifyFiltersAsync(WebHook webHook);

        /// <summary>
        /// Verifies the <see cref="WebHook.WebHookUri"/> by issuing an HTTP GET request to the provided 
        /// <paramref name="webHook"/> to ensure that it is reachable and expects WebHooks. The WebHook 
        /// validation response is expected to echo the contents of the <c>echo</c> query parameter unless
        /// the WebHook URI has a <c>NoEcho</c> query parameter.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify.</param>
        Task VerifyAddressAsync(WebHook webHook);
    }
}
