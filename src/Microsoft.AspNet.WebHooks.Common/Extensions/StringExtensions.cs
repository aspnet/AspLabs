// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class StringExtensions
    {
        /// <summary>
        /// Splits a string into segments based on a given <paramref name="separator"/>. The segments are 
        /// trimmed and empty segments containing only white space are removed.
        /// </summary>
        /// <param name="input">The string to split.</param>
        /// <param name="separator">An array of Unicode characters that delimit the substrings in this instance, an empty array that contains no delimiters, or null.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the resulting segments.</returns>
        public static string[] SplitAndTrim(this string input, params char[] separator)
        {
            if (input == null)
            {
                return new string[0];
            }

            return input.Split(separator).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }
    }
}
