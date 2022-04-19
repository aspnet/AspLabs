using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "Need to test collection implementation")]
    public class StringValuesNameValueCollectionTests
    {
        private readonly Fixture _fixture;

        public StringValuesNameValueCollectionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void OneItem()
        {
            // Arrange
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            var items = new Builder
            {
                { key, value },
            };

            // Act
            var collection = new StringValuesReadOnlyDictionaryNameValueCollection(items);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(value, collection.Get(key));
            Assert.Equal(new[] { value }, collection.GetValues(key));
        }

        [Fact]
        public void OneItemMultipleKeyValues()
        {
            // Arrange
            var key = _fixture.Create<string>();
            var value1 = _fixture.Create<string>();
            var value2 = _fixture.Create<string>();

            var items = new Builder
            {
                { key, StringValues.Concat(new(value1), value2) },
            };

            // Act
            var collection = new StringValuesReadOnlyDictionaryNameValueCollection(items);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal($"{value1},{value2}", collection.Get(key));
            Assert.Equal(new[] { value1, value2 }, collection.GetValues(key));
        }

        [Fact]
        public void IsReadOnly()
        {
            // Arrange
            var collection = new StringValuesReadOnlyDictionaryNameValueCollection();

            // Act/Assert
            Assert.Throws<NotSupportedException>(() => collection.Add(_fixture.Create<string>(), _fixture.Create<string>()));
        }

        private class Builder : IReadOnlyDictionary<string, StringValues>
        {
            private List<KeyValuePair<string, StringValues>> _items = new();

            public StringValues this[string key] => _items.First(i => i.Key == key).Value;

            public IEnumerable<string> Keys => _items.Select(i => i.Key);

            public IEnumerable<StringValues> Values => _items.Select(i => i.Value);

            public int Count => _items.Count;

            public void Add(string key, StringValues values) => _items.Add(new(key, values));

            public bool ContainsKey(string key) => _items.Any(i => i.Key == key);

            public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => _items.GetEnumerator();

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value)
            {
                var result = _items.FirstOrDefault(i => i.Key == key);

                if (result.Key is null)
                {
                    value = default(StringValues);
                    return false;
                }
                else
                {
                    value = result.Value;
                    return true;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
