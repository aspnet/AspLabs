// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UriExtensions
    {
        /// <summary>
        /// Checks to see if the URI scheme is 'https'. The check is case-insensitive.
        /// </summary>
        /// <param name="input">The URI to verify.</param>
        /// <returns><c>true</c> if URI scheme is 'https'; false otherwise.</returns>
        public static bool IsHttps(this Uri input)
        {
            return input != null && string.Equals(input.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        }
    }
}
