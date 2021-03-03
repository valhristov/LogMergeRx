using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;

namespace LogMergeRx
{
    public static class ObservableCollectionExtensions
    {
        public static IObservable<NotifyCollectionChangedEventArgs> ToObservable(this INotifyCollectionChanged collection) =>
            Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Select(x => x.EventArgs);
    }
}
