using System.Collections;
using System.Collections.Generic;
using AutoFixture;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace System.Web.Internal
{
    [Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "Need to test collection implementation")]
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
            var collection = new StringValuesNameValueCollection(items);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(key, collection.GetKey(0));
            Assert.Equal(value, collection.Get(0));
            Assert.Equal(new[] { value }, collection.GetValues(0));
        }

        [Fact]
        public void OneItemDuplicateKey()
        {
            // Arrange
            var key = _fixture.Create<string>();
            var value1 = _fixture.Create<string>();
            var value2 = _fixture.Create<string>();

            var items = new Builder
            {
                { key, value1 },
                { key, value2 },
            };

            // Act
            var collection = new StringValuesNameValueCollection(items);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(key, collection.GetKey(0));
            Assert.Equal($"{value1},{value2}", collection.Get(0));
            Assert.Equal(new[] { value1, value2 }, collection.GetValues(0));
        }

        [Fact]
        public void OneItemMultipleStringValue()
        {
            // Arrange
            var key = _fixture.Create<string>();
            var value1 = _fixture.Create<string>();
            var value2 = _fixture.Create<string>();
            var value3 = _fixture.Create<string>();

            var items = new Builder
            {
                { key, new StringValues(new[] { value1, value2 }) },
                { key, value3 },
            };

            // Act
            var collection = new StringValuesNameValueCollection(items);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(key, collection.GetKey(0));
            Assert.Equal($"{value1},{value2},{value3}", collection.Get(0));
            Assert.Equal(new[] { value1, value2, value3 }, collection.GetValues(0));
        }

        [Fact]
        public void IsReadOnly()
        {
            // Arrange
            var collection = new StringValuesNameValueCollection();

            // Act/Assert
            Assert.Throws<NotSupportedException>(() => collection.Add(_fixture.Create<string>(), _fixture.Create<string>()));
        }

        private class Builder : IEnumerable<KeyValuePair<string, StringValues>>
        {
            private List<KeyValuePair<string, StringValues>> _items = new();

            public void Add(string key, StringValues values) => _items.Add(new(key, values));

            public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => _items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
