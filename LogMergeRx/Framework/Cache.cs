using System;
using System.Collections.Concurrent;

namespace LogMergeRx
{
    public class Cache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _items =
            new ConcurrentDictionary<TKey, Lazy<TValue>>();

        private readonly Func<TKey, TValue> _factory;

        public Cache(Func<TKey, TValue> factory)
        {
            _factory = factory;
        }

        public TValue Get(TKey key) =>
            _items.GetOrAdd(key, x => new Lazy<TValue>(_factory(x))).Value;

        public TValue Remove(TKey key) =>
            _items.TryRemove(key, out var lazy) && lazy.IsValueCreated
                ? lazy.Value
                : default;
    }
}
