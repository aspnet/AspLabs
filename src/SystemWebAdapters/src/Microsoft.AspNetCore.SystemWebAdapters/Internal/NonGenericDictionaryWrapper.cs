// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal
{
    internal class NonGenericDictionaryWrapper : IDictionary
    {
        private readonly IDictionary<object, object?> _original;

        private ICollection? _keys;
        private ICollection? _values;

        public NonGenericDictionaryWrapper(IDictionary<object, object?> original)
        {
            _original = original;
        }

        public object? this[object key]
        {
            get => _original.TryGetValue(key, out var value) ? value : null;
            set => _original[key] = value!;
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => _original.IsReadOnly;

        public ICollection Keys
        {
            get
            {
                if (_keys is null)
                {
                    _keys = _original.Keys.AsNonGeneric();
                }

                return _keys;
            }
        }

        public ICollection Values
        {
            get
            {
                if (_values is null)
                {
                    _values = _original.Values.AsNonGeneric();
                }

                return _values;
            }
        }

        public int Count => _original.Count;

        public bool IsSynchronized => false;

        public object SyncRoot => null!;

        public void Add(object key, object? value) => _original.Add(key, value);

        public void Clear() => _original.Clear();

        public bool Contains(object key) => _original.ContainsKey(key);

        public void CopyTo(Array array, int index)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            foreach (var item in _original)
            {
                array.SetValue(new DictionaryEntry(item.Key, item.Value), index++);
            }
        }

        public void Remove(object key) => _original.Remove(key);

        public IDictionaryEnumerator GetEnumerator() => new DictionaryEnumerator(_original);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<object, object?>> _dictionary;

            public DictionaryEnumerator(IDictionary<object, object?> dictionary)
            {
                _dictionary = dictionary.GetEnumerator();
            }

            public DictionaryEntry Entry => new(_dictionary.Current.Key, _dictionary.Current.Value);

            public object Key => _dictionary.Current.Key;

            public object? Value => _dictionary.Current.Value;

            public object? Current => Entry;

            public bool MoveNext() => _dictionary.MoveNext();

            public void Reset() => _dictionary.Reset();
        }
    }
}
