using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using Xunit;

namespace System.Web.Internal
{
    [SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "Testing collection implementation")]
    public class NonGenericCollectionWrapperTests
    {
        private readonly Fixture _fixture;

        public NonGenericCollectionWrapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Count()
        {
            var original = _fixture.CreateMany<int>().ToArray();
            var wrapped = new NonGenericCollectionWrapper<int>(original);

            Assert.Equal(original.Length, wrapped.Count);
        }

        [Fact]
        public void Empty()
        {
            var wrapped = new NonGenericCollectionWrapper<object>(Array.Empty<object>());

            Assert.Equal(0, wrapped.Count);
            Assert.Empty(wrapped);
        }

        [Fact]
        public void SyncProperties()
        {
            var wrapped = new NonGenericCollectionWrapper<object>(Array.Empty<object>());

            Assert.False(wrapped.IsSynchronized);
            Assert.Null(wrapped.SyncRoot);
        }

        [Fact]
        public void CopyToNull()
        {
            var wrapped = new NonGenericCollectionWrapper<object>(Array.Empty<object>());

            Assert.Throws<ArgumentNullException>(() => wrapped.CopyTo(null!, 0));
        }

        [Fact]
        public void CopyTo()
        {
            var original = _fixture.CreateMany<int>().ToArray();
            var wrapped = new NonGenericCollectionWrapper<int>(original);
            var destination = new int[original.Length];

            wrapped.CopyTo(destination, 0);

            Assert.True(original.SequenceEqual(destination));
        }
    }
}
