using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace LogMergeRx
{
    public class CircularBuffer<T> : ICollection<T>, INotifyCollectionChanged
    {
        private readonly Queue<T> _inner;
        private readonly int _maxSize;

        public CircularBuffer(int maxSize = 1000)
            : this(new Queue<T>(), maxSize)
        {
        }

        public CircularBuffer(IEnumerable<T> items, int maxSize = 1000)
            : this (new Queue<T>(items.Reverse().Take(maxSize)), maxSize)
        {
        }

        private CircularBuffer(Queue<T> queue, int maxSize)
        {
            _inner = queue;
            _maxSize = maxSize;
        }

        public int Count { get; }

        public bool IsReadOnly { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            var removed = _inner.Count == _maxSize
                ? _inner.Dequeue()
                : default;

            _inner.Enqueue(item);

            var args = removed == null
                ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item)
                : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new List<T> { item }, new List<T> { removed });

            CollectionChanged?.Invoke(this, args);
        }

        public void Clear()
        {
            _inner.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void AddRange(IList<T> items)
        {
            var removed = Enumerable.Repeat(0, _inner.Count + items.Count - _maxSize)
                .Select(x => _inner.Dequeue())
                .ToList();

            foreach (var item in items)
            {
                _inner.Enqueue(item);
            }

            var args = removed.Count == 0
                ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items)
                : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, items, removed);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public bool Contains(T item) =>
            _inner.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) =>
            _inner.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() =>
            _inner.GetEnumerator();

        public bool Remove(T item) =>
            throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
