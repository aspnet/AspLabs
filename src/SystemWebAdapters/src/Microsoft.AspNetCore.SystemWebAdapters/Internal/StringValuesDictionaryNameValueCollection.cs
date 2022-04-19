using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal
{
    internal class StringValuesDictionaryNameValueCollection : WrappingNameValueCollection
    {
        private readonly IDictionary<string, StringValues> _values;

        public StringValuesDictionaryNameValueCollection(IDictionary<string, StringValues> headers)
        {
            _values = headers;
        }

        public override string?[] AllKeys => _values.Keys.ToArray();

        public override int Count => _values.Count;

        public override void Add(string? name, string? value)
        {
            if (name is null)
            {
                return;
            }

            if (_values.TryGetValue(name, out var existing))
            {
                _values[name] = StringValues.Concat(existing, value);
            }
            else
            {
                _values.Add(name, value);
            }
        }

        public override string[]? GetValues(string? name)
            => name is not null && _values.TryGetValue(name, out var values) ? values : default;

        public override void Remove(string? name)
        {
            if (name is not null)
            {
                _values.Remove(name);
            }
        }

        public override void Set(string? name, string? value)
        {
            if (name is null)
            {
                return;
            }

            _values[name] = value;
        }

        public override string? Get(string? name)
            => name is not null && _values.TryGetValue(name, out var values) ? values : default;

        public override void Clear() => _values.Clear();

        public override IEnumerator GetEnumerator() => _values.Keys.GetEnumerator();
    }
}
