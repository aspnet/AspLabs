// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;

namespace System.Collections.Generic
{
    /// <summary>
    /// Extension methods for various collections.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds a list of values to a given collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="collection">A collection to add values to.</param>
        /// <param name="values">The values to be added to the collection</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            if (values != null)
            {
                foreach (var value in values)
                {
                    collection.Add(value);
                }
            }
        }
    }
}
