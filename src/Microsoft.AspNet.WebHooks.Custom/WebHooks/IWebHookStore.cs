// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for managing persistent <see cref="WebHook"/> instances. WebHooks are 
    /// managed on a per user basis where each user is identified by a string. This can for example 
    /// be the user ID associated with a token or some other unique user identifier.
    /// </summary>
    public interface IWebHookStore
    {
        /// <summary>
        /// Gets all registered <see cref="WebHook"/> instances for a given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user for which to get the registered <see cref="WebHook"/> instances.</param>
        /// <returns>All registered <see cref="WebHook"/> instances for this user.</returns>
        Task<ICollection<WebHook>> GetAllWebHooksAsync(string user);

        /// <summary>
        /// Gets all active <see cref="WebHook"/> instances registered for a given user where the WebHook filters 
        /// match one of more of the given <paramref name="actions"/>.
        /// </summary>
        /// <param name="user">The user for which to query the registered <see cref="WebHook"/> instances.</param>
        /// <param name="actions">The set of actions that determines matching <see cref="WebHook"/> instances.</param>
        /// <param name="predicate">An optional function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>A collection of matching (and active) <see cref="WebHook"/> instances.</returns>
        Task<ICollection<WebHook>> QueryWebHooksAsync(string user, IEnumerable<string> actions, Func<WebHook, string, bool> predicate);

        /// <summary>
        /// Looks up an existing <see cref="WebHook"/> for a given <paramref name="user"/>. If a <see cref="WebHook"/>
        /// with this <paramref name="id"/> is not present then <c>null</c> is returned.
        /// </summary>
        /// <param name="user">The user for which to lookup a registered <see cref="WebHook"/> instance.</param>
        /// <param name="id">The ID uniquely identifying the WebHook.</param>
        /// <returns>The <see cref="WebHook"/> instance or <c>null</c>.</returns>
        Task<WebHook> LookupWebHookAsync(string user, string id);

        /// <summary>
        /// Registers a new <see cref="WebHook"/> for a given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user for which to register the <see cref="WebHook"/>.</param>
        /// <param name="webHook">The <see cref="WebHook"/> to register.</param>
        /// <returns>A <see cref="StoreResult"/> indicating the result of the operation.</returns>
        Task<StoreResult> InsertWebHookAsync(string user, WebHook webHook);

        /// <summary>
        /// Updates an existing <see cref="WebHook"/> for a given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user for which to update the <see cref="WebHook"/>.</param>
        /// <param name="webHook">The <see cref="WebHook"/> to update.</param>
        /// <returns>A <see cref="StoreResult"/> indicating the result of the operation.</returns>
        Task<StoreResult> UpdateWebHookAsync(string user, WebHook webHook);

        /// <summary>
        /// Deletes a registered <see cref="WebHook"/> for a given <paramref name="user"/>. If a <see cref="WebHook"/> 
        /// with the given <paramref name="id"/> is not found then the method returns <see cref="StoreResult.NotFound"/>.
        /// </summary>
        /// <param name="user">The user for which to delete the <see cref="WebHook"/>.</param>
        /// <param name="id">The ID uniquely identifying the WebHook.</param>
        /// <returns>A <see cref="StoreResult"/> indicating the result of the operation.</returns>
        Task<StoreResult> DeleteWebHookAsync(string user, string id);

        /// <summary>
        /// Deletes all existing <see cref="WebHook"/> instances for a given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user for which to delete all <see cref="WebHook"/> instances.</param>
        Task DeleteAllWebHooksAsync(string user);

        /// <summary>
        /// Gets all active <see cref="WebHook"/> instances across all users with subscriptions that match the given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="actions">The set of actions that determines matching <see cref="WebHook"/> instances.</param>
        /// <param name="predicate">An optional function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>A collection of matching (and active) <see cref="WebHook"/> instances.</returns>
        Task<ICollection<WebHook>> QueryWebHooksAcrossAllUsersAsync(IEnumerable<string> actions, Func<WebHook, string, bool> predicate);
    }
}
