// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.Caching;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Xunit;

namespace System.Web.Caching;

public class CacheTests
{
    private readonly Fixture _fixture;

    public CacheTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void GetEmpty()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>());
        var cache = new Cache(memCache);

        // Act
        var result = cache[_fixture.Create<string>()];

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Count()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>())
        {
            { _fixture.Create<string>(), new object(), DateTimeOffset.MaxValue },
            { _fixture.Create<string>(), new object(), DateTimeOffset.MaxValue },
            { _fixture.Create<string>(), new object(), DateTimeOffset.MaxValue },
        };

        var cache = new Cache(memCache);

        // Act
        var result = cache.Count;

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void Enumerator()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = new object();

        using var memCache = new MemoryCache(_fixture.Create<string>())
        {
            { key, value, DateTimeOffset.MaxValue },
        };

        var cache = new Cache(memCache);

        // Act
        var result = cache.GetEnumerator();

        // Assert
        var enumerator = (IDictionaryEnumerator)result;

        Assert.True(enumerator.MoveNext());
        Assert.Equal(key, enumerator.Key);
        Assert.Equal(value, enumerator.Value);

        Assert.False(result.MoveNext());
        result.Reset();

        Assert.True(result.MoveNext());
        Assert.Equal(key, enumerator.Key);
        Assert.Equal(value, enumerator.Value);
    }

    [Fact]
    public void InsertNoCallbacks()
    {
        // Arrange
        var memCache = new Mock<MemoryCache>(_fixture.Create<string>(), null);
        var cache = new Cache(memCache.Object);
        var key = _fixture.Create<string>();
        var item = new object();
        var absoluteExpriration = _fixture.Create<DateTime>();
        var slidingExpiration = _fixture.Create<TimeSpan>();

        // Act
        cache.Insert(key, item, null, absoluteExpriration, slidingExpiration);

        // Assert
        memCache.Verify(m => m.Set(key, item, It.Is<CacheItemPolicy>(e => e.AbsoluteExpiration.Equals(absoluteExpriration) && e.SlidingExpiration.Equals(slidingExpiration)), null), Times.Once);
    }

    [Fact]
    public void InsertNoCallbacksConstants()
    {
        // Arrange
        var memCache = new Mock<MemoryCache>(_fixture.Create<string>(), null);
        var cache = new Cache(memCache.Object);
        var key = _fixture.Create<string>();
        var item = new object();

        // Act
        cache.Insert(key, item, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

        // Assert
        memCache.Verify(m => m.Set(key, item, It.Is<CacheItemPolicy>(e => e.AbsoluteExpiration.Equals(DateTimeOffset.MaxValue) && e.SlidingExpiration.Equals(Cache.NoSlidingExpiration)), null), Times.Once);
    }

    [InlineData(CacheItemPriority.Low, Runtime.Caching.CacheItemPriority.Default)]
    [InlineData(CacheItemPriority.BelowNormal, Runtime.Caching.CacheItemPriority.Default)]
    [InlineData(CacheItemPriority.Normal, Runtime.Caching.CacheItemPriority.Default)]
    [InlineData(CacheItemPriority.AboveNormal, Runtime.Caching.CacheItemPriority.Default)]
    [InlineData(CacheItemPriority.High, Runtime.Caching.CacheItemPriority.Default)]
    [InlineData(CacheItemPriority.NotRemovable, Runtime.Caching.CacheItemPriority.NotRemovable)]
    [Theory]
    public void InsertPriority(CacheItemPriority webPriority, Runtime.Caching.CacheItemPriority runtimePriority)
    {
        // Arrange
        var memCache = new Mock<MemoryCache>(_fixture.Create<string>(), null);
        var cache = new Cache(memCache.Object);
        var key = _fixture.Create<string>();
        var item = new object();
        var absoluteExpriration = _fixture.Create<DateTime>();
        var slidingExpiration = _fixture.Create<TimeSpan>();

        // Act
        cache.Insert(key, item, null, absoluteExpriration, slidingExpiration, webPriority, onRemoveCallback: null);

        // Assert
        memCache.Verify(m => m.Set(key, item, It.Is<CacheItemPolicy>(e => e.AbsoluteExpiration.Equals(absoluteExpriration) && e.SlidingExpiration.Equals(slidingExpiration) && e.Priority == runtimePriority), null), Times.Once);
    }

    [Fact]
    public void InsertPriorityConstants()
    {
        // Arrange
        var memCache = new Mock<MemoryCache>(_fixture.Create<string>(), null);
        var cache = new Cache(memCache.Object);
        var key = _fixture.Create<string>();
        var item = new object();

        // Act
        cache.Insert(key, item, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, onRemoveCallback: null);

        // Assert
        memCache.Verify(m => m.Set(key, item, It.Is<CacheItemPolicy>(e => e.AbsoluteExpiration.Equals(DateTimeOffset.MaxValue) && e.SlidingExpiration.Equals(Cache.NoSlidingExpiration) && e.Priority == Runtime.Caching.CacheItemPriority.Default), null), Times.Once);
    }

    [Fact]
    public void AddItemIndexer()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>());
        var cache = new Cache(memCache);
        var item = new object();
        var key = _fixture.Create<string>();

        // Act
        cache[key] = item;

        // Assert
        Assert.Same(item, cache[key]);
    }

    [Fact]
    public void AddItemThenRemove()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>());
        var cache = new Cache(memCache);
        var item = new object();
        var item2 = new object();
        var key = _fixture.Create<string>();
        Removal? removed = null;

        // Act
        var first = cache.Add(key, item, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, (key, item, reason) => removed = new(key, item, reason));
        cache.Remove(key);

        // Assert
        Assert.Null(first);
        Assert.NotNull(removed);
        Assert.Equal(key, removed!.Key);
        Assert.Equal(item, removed.Item);
        Assert.Equal(CacheItemRemovedReason.Removed, removed.Reason);
    }

    [Fact]
    public async Task UpdateItemCallback()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>());
        var cache = new Cache(memCache);
        var item = new object();
        var item2 = new object();
        var key = _fixture.Create<string>();
        var updated = false;
        var slidingExpiration = TimeSpan.FromMilliseconds(1);
        CacheItemUpdateReason? updateReason = default;

        void Callback(string key, CacheItemUpdateReason reason, out object? expensiveObject, out CacheDependency? dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration)
        {
            expensiveObject = item2;
            dependency = null;
            absoluteExpiration = Cache.NoAbsoluteExpiration;
            slidingExpiration = TimeSpan.FromMilliseconds(5);

            updated = true;
            updateReason = reason;
        }

        // Act
        cache.Insert(key, item, null, Cache.NoAbsoluteExpiration, slidingExpiration, Callback);

        // Ensure sliding expiration has hit
        await Task.Delay(slidingExpiration);

        // Force cleanup to initiate callbacks on current thread
        memCache.Trim(100);

        // Assert
        Assert.True(updated);
        Assert.Same(cache[key], item2);
        Assert.Equal(CacheItemUpdateReason.Expired, updateReason);
    }

    [Fact]
    public async Task UpdateItemCallbackRemove()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>());
        var cache = new Cache(memCache);
        var item = new object();
        var key = _fixture.Create<string>();
        var updated = false;
        var slidingExpiration = TimeSpan.FromMilliseconds(1);
        CacheItemUpdateReason? updateReason = default;

        void Callback(string key, CacheItemUpdateReason reason, out object? expensiveObject, out CacheDependency? dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration)
        {
            expensiveObject = null;
            dependency = null;
            absoluteExpiration = Cache.NoAbsoluteExpiration;
            slidingExpiration = TimeSpan.FromMilliseconds(5);

            updated = true;
            updateReason = reason;
        }

        // Act
        cache.Insert(key, item, null, Cache.NoAbsoluteExpiration, slidingExpiration, Callback);

        // Ensure sliding expiration has hit
        await Task.Delay(slidingExpiration);

        // Force cleanup to initiate callbacks on current thread
        memCache.Trim(100);

        // Assert
        Assert.True(updated);
        Assert.Null(cache[key]);
        Assert.Equal(CacheItemUpdateReason.Expired, updateReason);
    }

    [Fact]
    public void InsertItem()
    {
        // Arrange
        using var memCache = new MemoryCache(_fixture.Create<string>());
        var cache = new Cache(memCache);
        var item = new object();
        var key = _fixture.Create<string>();

        // Act
        cache[key] = item;

        // Assert
        Assert.Same(item, cache[key]);
    }

    private record Removal(string Key, object Item, CacheItemRemovedReason Reason);
}
