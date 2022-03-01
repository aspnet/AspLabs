using System;
using System.Web;
using System.Web.Caching;

namespace ClassLibrary
{
    public static class Helper
    {
        private const string Key = "CachedItem";

        public static string UserAgent => HttpContext.Current.Request.UserAgent;

        public static string GetUserAgent(HttpContextBase context) => context.Request.UserAgent;

        public static LargeItem Item
        {
            get
            {
                if (HttpContext.Current.Cache[Key] is LargeItem item)
                {
                    return item;
                }

                var largeItem = LargeItem.GetNext();

                HttpContext.Current.Cache.Insert(Key, largeItem, dependencies: null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(5), OnUpdate);

                return largeItem;

                static void OnUpdate(string key, CacheItemUpdateReason reason, out object expensiveObject, out CacheDependency dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration)
                {
                    expensiveObject = LargeItem.GetNext();
                    dependency = null;
                    absoluteExpiration = Cache.NoAbsoluteExpiration;
                    slidingExpiration = TimeSpan.FromSeconds(5);
                }
            }
        }
    }

    public class LargeItem
    {
        private static long _count;

        private LargeItem(long count)
        {
            Count = count;
        }

        public static LargeItem GetNext() => new(_count++);

        public long Count { get; }
    }
}
