using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace LogMergeRx
{
    public static class ObservableExtensions
    {
        public static IObservable<Unit> ToUnit<T>(this IObservable<T> observable) =>
            observable.Select(_ => Unit.Default);

        public static IObservable<object> ToObject<T>(this IObservable<T> observable) =>
            observable.Select(_ => default(object));
    }
}
