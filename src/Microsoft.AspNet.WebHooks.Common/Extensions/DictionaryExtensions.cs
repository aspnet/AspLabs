// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;

namespace System.Collections.Generic
{
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <see cref="string"/> and <c>TValue</c> of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value with the specified key if found; otherwise the default value.</param>
        /// <returns><c>true</c> if key was found and value is of type <typeparamref name="TValue"/> and non-null; otherwise false.</returns>
        public static bool TryGetValue<TValue>(this IDictionary<string, object> dictionary, string key, out TValue value)
        {
            if (dictionary != null)
            {
                object valueAsObj;
                if (dictionary.TryGetValue(key, out valueAsObj))
                {
                    if (valueAsObj is TValue)
                    {
                        value = (TValue)valueAsObj;
                        return true;
                    }
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> with the given key, or the <c>default</c> value 
        /// if the key is not present or the value is not of type <typeparamref name="TValue"/>. 
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> instance <c>TKey</c> is of type <see cref="string"/> and <c>TValue</c> of type <see cref="object"/>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>The value with the specified key if found; otherwise the default value.</returns>
        public static TValue GetValueOrDefault<TValue>(this IDictionary<string, object> dictionary, string key)
        {
            TValue value;
            TryGetValue(dictionary, key, out value);
            return value;
        }

        /// <summary>
        /// Sets the entry with the given key to the given value. If value is the default value
        /// then the entry is removed.
        /// </summary>
        /// <typeparam name="T">Type of value to be set or cleared.</typeparam>
        /// <param name="dictionary">The dictionary to insert of clear a value from.</param>
        /// <param name="key">The key of the entry.</param>
        /// <param name="value">The value (or default value).</param>
        public static void SetOrClearValue<T>(this IDictionary<string, object> dictionary, string key, T value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                dictionary.Remove(key);
            }
            else
            {
                dictionary[key] = value;
            }
        }
    }
}
