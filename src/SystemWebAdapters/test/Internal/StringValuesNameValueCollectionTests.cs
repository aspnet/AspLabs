using AutoFixture;
using Microsoft.Extensions.Primitives;
using Xunit;

using StringValuesDictionary = System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>;

namespace System.Web.Internal
{
    public class StringValuesNameValueCollectionTests
    {
        private readonly Fixture _fixture;

        public StringValuesNameValueCollectionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void PlatformNotSupportedExceptions()
        {
            var dictionary = new StringValuesDictionary();
            var collection = new StringValuesNameValueCollection(dictionary);

            Assert.Throws<PlatformNotSupportedException>(() => collection.Keys);
            Assert.Throws<PlatformNotSupportedException>(() => collection.Get(0));
            Assert.Throws<PlatformNotSupportedException>(() => collection.GetValues(0));
            Assert.Throws<PlatformNotSupportedException>(() => collection.GetKey(0));
        }

        [Fact]
        public void Empty()
        {
            // Arrange
            var dictionary = new StringValuesDictionary();
            var collection = new StringValuesNameValueCollection(dictionary);

            // Act
            var result = collection.Count;

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void AddGetValue()
        {
            // Arrange
            var dictionary = new StringValuesDictionary();
            var collection = new StringValuesNameValueCollection(dictionary);

            var newKey = _fixture.Create<string>();
            var newValue1 = _fixture.Create<string>();
            var newValue2 = _fixture.Create<string>();

            var expected = new StringValues(new[] { newValue1, newValue2 });

            // Act
            collection.Add(newKey, newValue1);
            collection.Add(newKey, newValue2);

            // Assert
            Assert.Equal(expected.ToArray(), collection.GetValues(newKey));
            Assert.Equal(expected.ToString(), collection.Get(newKey));
            Assert.Equal(expected, dictionary[newKey]);
        }

        [Fact]
        public void SetValue()
        {
            // Arrange
            var key = _fixture.Create<string>();
            var dictionary = new StringValuesDictionary
            {
                { key, _fixture.Create<string>() }
            };
            var collection = new StringValuesNameValueCollection(dictionary);

            var newValue = _fixture.Create<string>();

            // Act
            collection.Set(key, newValue);

            // Assert
            Assert.Equal(newValue, collection.Get(key));
            Assert.Equal(newValue, dictionary[key]);
        }

        [Fact]
        public void GetKeyNotPresent()
        {
            // Arrange
            var dictionary = new StringValuesDictionary();
            var collection = new StringValuesNameValueCollection(dictionary);

            // Act
            var result = collection.Get(_fixture.Create<string>());
            var results = collection.GetValues(_fixture.Create<string>());

            // Assert
            Assert.Null(result);
            Assert.Null(results);
        }

            [Fact]
        public void HandleNullKey()
        {
            // Arrange
            var dictionary = new StringValuesDictionary();
            var collection = new StringValuesNameValueCollection(dictionary);

            // Act
            collection.Add(null, _fixture.Create<string>());

            // Assert
            Assert.Empty(collection);
            Assert.Empty(dictionary);
        }

        [Fact]
        public void EmptyKeys()
        {
            // Arrange
            var dictionary = new StringValuesDictionary();
            var collection = new StringValuesNameValueCollection(dictionary);

            // Act
            var keys = collection.AllKeys;

            // Assert
            Assert.Empty(keys);
        }

        [Fact]
        public void AllKeys()
        {
            // Arrange
            var dictionary = new StringValuesDictionary
            {
                { _fixture.Create<string>(), _fixture.Create<string>() },
                { _fixture.Create<string>(), _fixture.Create<string>() },
                { _fixture.Create<string>(), _fixture.Create<string>() },
                { _fixture.Create<string>(), _fixture.Create<string>() },
            };

            var collection = new StringValuesNameValueCollection(dictionary);

            // Act
            var keys1 = collection.AllKeys;
            var keys2 = collection.AllKeys;

            // Assert
            Assert.NotSame(keys1, keys2);
            Assert.Equal(dictionary.Keys, keys1);
        }

        [Fact]
        public void Remove()
        {
            // Arrange
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            var dictionary = new StringValuesDictionary
            {
                { key, value }
            };

            var collection = new StringValuesNameValueCollection(dictionary);

            // Act
            collection.Remove(key);

            // Assert
            Assert.False(dictionary.ContainsKey(key));
        }

        [Fact]
        public void RemoveKeyNotPresent()
        {
            // Arrange
            var dictionary = new StringValuesDictionary
            {
                { _fixture.Create<string>(), _fixture.Create<string>() }
            };

            var collection = new StringValuesNameValueCollection(dictionary);

            // Act
            collection.Remove(_fixture.Create<string>());

            // Assert
            Assert.Single(collection);
            Assert.Single(dictionary);
        }

        [Fact]
        public void Clear()
        {
            // Arrange
            var dictionary = new StringValuesDictionary
            {
                { _fixture.Create<string>(), _fixture.Create<string>() }
            };

            var collection = new StringValuesNameValueCollection(dictionary);

            // Act
            collection.Clear();

            // Assert
            Assert.Empty(collection);
            Assert.Empty(dictionary);
        }

        [Fact]
        public void GetEnumerator()
        {
            // Arrange
            var key = _fixture.Create<string>();
            var dictionary = new StringValuesDictionary
            {
                { key, _fixture.Create<string>()}
            };

            var collection = new StringValuesNameValueCollection(dictionary);
            var enumerator = collection.GetEnumerator();

            // Act/Assert
            Assert.True(enumerator.MoveNext());
            Assert.Equal(key, enumerator.Current);
            Assert.False(enumerator.MoveNext());
            enumerator.Reset();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(key, enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }
    }
}
