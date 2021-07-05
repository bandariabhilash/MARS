using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;

namespace FarmerBrothers.CacheManager
{
    public class MemoryCacheManager
    {
        private static ObjectCache Cache;

        static MemoryCacheManager()
        {
            Cache = MemoryCache.Default;
        }

        protected static object Get(string key, string prefix)
        {
            return Cache[key + prefix];
        }

        protected static void Set(string key, string prefix, object data)
        {
            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = new TimeSpan(24, 0, 0) };
            Cache.Add(new CacheItem(key + prefix, data), policy);
        }

        protected static bool Has(string key, string prefix)
        {
            return (Cache[key + prefix] != null);
        }

        protected static void Remove(string key, string prefix)
        {
            Cache.Remove(key + prefix);
        }
    }
}