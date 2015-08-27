// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Extension methods for <see cref="WebHook"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookExtensions
    {
        /// <summary>
        /// Determines whether a given <paramref name="action"/> matches the filters for a given <see cref="WebHook"/>.
        /// The action can either match a filter directly or match a wildcard.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> instance to operate on.</param>
        /// <param name="action">The action to match against the <paramref name="webHook"/> filters.</param>
        /// <returns><c>true</c> if the <paramref name="action"/> matches, otherwise <c>false</c>.</returns>
        public static bool MatchesAction(this WebHook webHook, string action)
        {
            return webHook != null && (webHook.Filters.Contains(WildcardWebHookFilterProvider.Name) || webHook.Filters.Contains(action));
        }
    }
}