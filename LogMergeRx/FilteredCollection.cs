using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;

namespace LogMergeRx
{
    public class FilteredCollection<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly List<T> _hidden = new List<T>();
        private readonly List<T> _visible = new List<T>();

        public ObservableProperty<Func<T, bool>> Filter { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Count => _visible.Count;

        public bool IsReadOnly => false;

        public FilteredCollection(IEnumerable<T> items)
        {
            Filter = new ObservableProperty<Func<T, bool>>(T => true, AlwaysNotEqualComparer<Func<T, bool>>.Default);

            Filter.Subscribe(filter => Refresh(filter));

            AddRange(items);
        }

        public FilteredCollection() : this(Enumerable.Empty<T>())
        {
        }

        public void Refresh(Func<T, bool> filter)
        {
            var toShow = new List<T>(_hidden.Where(x => filter(x)));
            var toHide = new List<T>(_visible.Where(x => !filter(x)));

            foreach (var item in toHide)
            {
                _visible.Remove(item);
            }

            foreach (var item in toShow)
            {
                _hidden.Remove(item);
            }

            _visible.AddRange(toShow);
            _hidden.AddRange(toHide);

            if (toShow.Count > 0 && toHide.Count > 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, toShow, toHide));
            }
            else if (toShow.Count > 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, toShow));
            }
            else if (toHide.Count > 0)
            {
                foreach (var item in toHide)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        public void AddRange(params T[] items) =>
            AddRange(items.AsEnumerable());

        public void AddRange(IEnumerable<T> items)
        {
            var groups = items.GroupBy(Filter.Value);

            var added = groups.Where(g => g.Key == true).SelectMany(g => g).ToList();
            _visible.AddRange(added);

            var hidden = groups.Where(g => g.Key == false).SelectMany(g => g).ToList();
            _hidden.AddRange(hidden);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
        }

        public void Add(T item)
        {
            if (Filter.Value(item))
            {
                _visible.Add(item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
            else
            {
                _hidden.Add(item);
            }
        }

        public void Clear()
        {
            _visible.Clear();
            _hidden.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item) =>
            _visible.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) =>
            _visible.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            if (_visible.Remove(item))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                return true;
            }
            else
            {
                return _hidden.Remove(item);
            }
        }

        public IEnumerator<T> GetEnumerator() =>
            _visible.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private class AlwaysNotEqualComparer<TValue> : IEqualityComparer<TValue>
        {
            public static IEqualityComparer<TValue> Default { get; } = new AlwaysNotEqualComparer<TValue>();

            public bool Equals([AllowNull] TValue x, [AllowNull] TValue y) =>
                false;

            public int GetHashCode([DisallowNull] TValue obj) =>
                obj?.GetHashCode() ?? 0;
        }
    }
}
