using System;
using System.Collections.Generic;

namespace LogMergeRx
{
    public class ObservableBase<T> : IObservable<T>
    {
        private readonly HashSet<IObserver<T>> _observers = new HashSet<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new Subscription(() => _observers.Remove(observer));
        }

        protected void NotifyObservers(T value)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(value);
            }
        }

        private class Subscription : IDisposable
        {
            private Action _finish;

            public Subscription(Action finish)
            {
                _finish = finish;
            }

            public void Dispose()
            {
                _finish?.Invoke();
                _finish = null;
            }
        }
    }
}
