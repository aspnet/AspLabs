using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace System.Web.Internal
{
    internal class StringValuesNameValueCollection : NameValueCollection
    {
        private const string GetByIntNotSupported = "Get by index is not available on this platform";

        private readonly IDictionary<string, StringValues> _headers;

        public StringValuesNameValueCollection(IDictionary<string, StringValues> headers)
        {
            _headers = headers;
        }

        public override string?[] AllKeys => _headers.Keys.ToArray();

        public override int Count => _headers.Count;

        public override void Add(string? name, string? value)
        {
            if (name is null)
            {
                return;
            }

            if (_headers.TryGetValue(name, out var existing))
            {
                _headers[name] = StringValues.Concat(existing, value);
            }
            else
            {
                _headers.Add(name, value);
            }
        }

        public override KeysCollection Keys => throw new PlatformNotSupportedException("KeysCollection is not supported as Get(int) is not available.");

        public override string? Get(int index) => throw new PlatformNotSupportedException(GetByIntNotSupported);

        public override string? GetKey(int index) => throw new PlatformNotSupportedException(GetByIntNotSupported);

        public override string[]? GetValues(int index) => throw new PlatformNotSupportedException(GetByIntNotSupported);

        public override string[]? GetValues(string? name)
        {
            if (name is not null && _headers.TryGetValue(name, out var values))
            {
                return values;
            }

            return null;
        }

        public override void Remove(string? name)
        {
            if (name is not null)
            {
                _headers.Remove(name);
            }
        }

        public override void Set(string? name, string? value)
        {
            if (name is null)
            {
                return;
            }

            _headers[name] = value;
        }

        public override string? Get(string? name)
        {
            if (name is not null && _headers.TryGetValue(name, out var value))
            {
                return value;
            }

            return null;
        }

        public override void Clear() => _headers.Clear();

        public override IEnumerator GetEnumerator() => _headers.Keys.GetEnumerator();
    }
}
