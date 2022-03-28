// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.Caching;

namespace System.Web.Caching;

public delegate void CacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason);
public delegate void CacheItemUpdateCallback(string key, CacheItemUpdateReason reason, out object? expensiveObject, out CacheDependency? dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration);

public class Cache : IEnumerable
{
    private readonly ObjectCache _cache;

    public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;

    public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

    public Cache()
        : this(MemoryCache.Default)
    {
    }

    public Cache(ObjectCache cache)
    {
        _cache = cache;
    }

    public object this[string key]
    {
        get => Get(key);
        set => Insert(key, value);
    }

    public object Add(string key, object value, CacheDependency? dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback? onRemoveCallback)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = Convert(absoluteExpiration),
            SlidingExpiration = slidingExpiration,
            Priority = Convert(priority),
            RemovedCallback = Convert(onRemoveCallback),
        };

        return _cache.AddOrGetExisting(key, value, policy);
    }

    public object Get(string key) => _cache.Get(key);

    public void Insert(string key, object value) => _cache.Set(key, value, DateTimeOffset.MaxValue);

    public void Insert(string key, object value, CacheDependency? dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = Convert(absoluteExpiration),
            SlidingExpiration = slidingExpiration,
        };

        _cache.Set(key, value, policy);
    }

    public void Insert(string key, object value, CacheDependency? dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback? onRemoveCallback)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = Convert(absoluteExpiration),
            SlidingExpiration = slidingExpiration,
            Priority = Convert(priority),
            RemovedCallback = Convert(onRemoveCallback),
        };

        _cache.Set(key, value, policy);
    }

    public void Insert(string key, object value, CacheDependency? dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemUpdateCallback onUpdateCallback)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = Convert(absoluteExpiration),
            SlidingExpiration = slidingExpiration,
            UpdateCallback = Convert(onUpdateCallback),
        };

        _cache.Set(key, value, policy);
    }

    public object? Remove(string key) => _cache.Remove(key);

    private static Runtime.Caching.CacheItemPriority Convert(CacheItemPriority priority) => priority switch
    {
        CacheItemPriority.NotRemovable => Runtime.Caching.CacheItemPriority.NotRemovable,
        _ => Runtime.Caching.CacheItemPriority.Default,
    };

    private static CacheItemRemovedReason Convert(CacheEntryRemovedReason reason) => reason switch
    {
        CacheEntryRemovedReason.Expired => CacheItemRemovedReason.Expired,
        CacheEntryRemovedReason.Evicted => CacheItemRemovedReason.Underused,
        CacheEntryRemovedReason.ChangeMonitorChanged => CacheItemRemovedReason.DependencyChanged,
        _ => CacheItemRemovedReason.Removed,
    };

    private static CacheEntryRemovedCallback? Convert(CacheItemRemovedCallback? callback)
    {
        if (callback is null)
        {
            return null;
        }

        return args => callback(args.CacheItem.Key, args.CacheItem.Value, Convert(args.RemovedReason));
    }

    private static DateTimeOffset Convert(DateTime dt) => dt == NoAbsoluteExpiration ? DateTimeOffset.MaxValue : dt;

    private static CacheEntryUpdateCallback? Convert(CacheItemUpdateCallback? callback)
    {
        if (callback is null)
        {
            return null;
        }

        return args =>
        {
            var reason = args.RemovedReason switch
            {
                CacheEntryRemovedReason.ChangeMonitorChanged => CacheItemUpdateReason.DependencyChanged,
                _ => CacheItemUpdateReason.Expired,
            };

            callback(args.Key, reason, out var expensiveObject, out _, out var absoluteExpiration, out var slidingExpiration);

            if (expensiveObject is null)
            {
                return;
            }

            args.UpdatedCacheItem = new(args.Key, expensiveObject);
            args.UpdatedCacheItemPolicy = new()
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = Convert(absoluteExpiration),
                UpdateCallback = Convert(callback),
            };
        };
    }

    public int Count => (int)_cache.GetCount();

    public IEnumerator GetEnumerator() => ((IEnumerable)_cache).GetEnumerator();
}
