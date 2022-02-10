using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using Moq;
using Xunit;

namespace System.Web.Internal
{
    [SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "Testing collection implementation")]
    public class NonGenericDictionaryWrapperTests
    {
        private readonly Fixture _fixture;

        public NonGenericDictionaryWrapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void IsFixedSize()
        {
            var dictionary = new Dictionary<object, object?>();
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.False(wrapped.IsFixedSize);
        }

        [Fact]
        public void IsSynchronized()
        {
            var dictionary = new Dictionary<object, object?>();
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.False(wrapped.IsSynchronized);
        }

        [Fact]
        public void SyncRoot()
        {
            var dictionary = new Dictionary<object, object?>();
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.Null(wrapped.SyncRoot);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsReadOnly(bool isReadOnly)
        {
            var dictionary = new Mock<IDictionary<object, object?>>();
            dictionary.Setup(x => x.IsReadOnly).Returns(isReadOnly);

            var wrapped = new NonGenericDictionaryWrapper(dictionary.Object);

            Assert.Equal(isReadOnly, wrapped.IsReadOnly);
        }

        [Fact]
        public void Keys()
        {
            var key1 = _fixture.Create<string>();
            var key2 = _fixture.Create<string>();

            var dictionary = new Dictionary<object, object?>
            {
                { key1, _fixture.Create<string>() },
                { key2, _fixture.Create<string>() },
            };
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.Equal(wrapped.Keys, new[] { key1, key2 });
        }

        [Fact]
        public void Values()
        {
            var value1 = _fixture.Create<string>();
            var value2 = _fixture.Create<string>();

            var dictionary = new Dictionary<object, object?>
            {
                { _fixture.Create<string>(), value1 },
                { _fixture.Create<string>(), value2 },
            };
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.Equal(wrapped.Values, new[] { value1, value2 });
        }

        [Fact]
        public void Count()
        {
            var dictionary = new Dictionary<object, object?>(_fixture.CreateMany<KeyValuePair<object, object?>>());
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.Equal(dictionary.Count, wrapped.Count);
        }

        [Fact]
        public void Indexer()
        {
            var dictionary = new Dictionary<object, object?>();
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            wrapped[key] = value;

            Assert.Equal(1, wrapped.Count);
            Assert.Equal(value, wrapped[key]);
            Assert.Equal(value, dictionary[key]);
        }

        [Fact]
        public void IndexerItemDoesNotExists()
        {
            var dictionary = new Dictionary<object, object?>();
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.Null(wrapped[new()]);
        }

        [Fact]
        public void Add()
        {
            var dictionary = new Dictionary<object, object?>();
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            wrapped.Add(key, value);

            Assert.Equal(value, wrapped[key]);
            Assert.Equal(value, dictionary[key]);
        }

        [Fact]
        public void Remove()
        {
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            var dictionary = new Dictionary<object, object?>
            {
                { key, value },
            };
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.Equal(1, wrapped.Count);
            Assert.Equal(1, dictionary.Count);

            wrapped.Remove(key);

            Assert.Equal(0, wrapped.Count);
            Assert.Equal(0, dictionary.Count);
        }

        [Fact]
        public void Clear()
        {
            var dictionary = new Dictionary<object, object?>(_fixture.CreateMany<KeyValuePair<object, object?>>());
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.NotEqual(0, wrapped.Count);

            wrapped.Clear();

            Assert.Equal(0, dictionary.Count);
            Assert.Equal(0, wrapped.Count);
        }

        [Fact]
        public void Contains()
        {
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            var dictionary = new Dictionary<object, object?>()
            {
                { key, value },
            };
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.True(wrapped.Contains(key));
        }

        [Fact]
        public void CopyToNull()
        {
            var dictionary = new Dictionary<object, object?>();
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            Assert.Throws<ArgumentNullException>(() => wrapped.CopyTo(null!, 0));
        }

        [Fact]
        public void CopyTo()
        {
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            var dictionary = new Dictionary<object, object?>()
            {
                { key, value },
            };
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            var array = new DictionaryEntry[1];
            wrapped.CopyTo(array, 0);

            Assert.Equal(key, array[0].Key);
            Assert.Equal(value, array[0].Value);
        }

        [Fact]
        public void GetEnumerator()
        {
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            var dictionary = new Dictionary<object, object?>()
            {
                { key, value },
            };
            var wrapped = new NonGenericDictionaryWrapper(dictionary);

            var enumerator = wrapped.GetEnumerator();

            Assert.True(enumerator.MoveNext());

            var entry = enumerator.Entry;

            Assert.Equal(key, enumerator.Key);
            Assert.Equal(value, enumerator.Value);
            Assert.Equal(key, entry.Key);
            Assert.Equal(value, entry.Value);
            Assert.Equal(entry, enumerator.Current);

            Assert.False(enumerator.MoveNext());
            enumerator.Reset();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(entry, enumerator.Entry);
        }
    }
}
