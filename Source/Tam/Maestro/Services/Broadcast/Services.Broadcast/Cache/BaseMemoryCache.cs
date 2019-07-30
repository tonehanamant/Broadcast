using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Threading;

namespace Services.Broadcast.Cache
{
    public class BaseMemoryCache<TItem>
    {
        private MemoryCache _Cache;
        private readonly ConcurrentDictionary<string, object> _KeyLocks = new ConcurrentDictionary<string, object>();

        public BaseMemoryCache(string cacheName = null)
        {
            _Cache = cacheName == null ? MemoryCache.Default : new MemoryCache(cacheName);
        }

        public TItem GetOrCreate(string key, Func<TItem> createItemForKeyFunc, CacheItemPolicy cacheItemPolicy)
        {
            TItem item;

            if (_Cache.Contains(key))
            {
                item = (TItem)_Cache[key];
            }
            else
            {
                var keyLock = _KeyLocks.GetOrAdd(key, k => new object());

                // lock all the other requests with the same key
                // so that delegate createItem is called only once for several parallel requests with the same key
                lock (keyLock)
                {
                    if (_Cache.Contains(key))
                    {
                        item = (TItem)_Cache[key];
                    }
                    else
                    {
                        // if it is still not in the cache, let's get it
                        item = createItemForKeyFunc();

                        _Cache.Add(key, item, cacheItemPolicy);
                    }
                }
            }

            return item;
        }

        public void Remove(string key)
        {
            if (_Cache.Contains(key))
            {
                _Cache.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            return _Cache.Contains(key);
        }

        public long GetItemCount(bool reset)
        {
            var itemCount = _Cache.GetCount();
            if (reset)
            {
                var oldCache = Interlocked.Exchange(ref _Cache, new MemoryCache(_Cache.Name));
                oldCache.Dispose();
            }
            return itemCount;
            
        }
    }
}
