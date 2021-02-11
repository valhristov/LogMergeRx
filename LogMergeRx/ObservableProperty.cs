using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LogMergeRx
{
    public class ObservableProperty<T> : IObservable<T>, INotifyPropertyChanged
    {
        private readonly HashSet<IObserver<T>> _observers = new HashSet<IObserver<T>>();

        private T _value;
        private readonly IEqualityComparer<T> _comparer;

        public T Value
        {
            get => _value;
            set
            {
                if (_comparer.Equals(_value, value))
                {
                    return;
                }
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                foreach (var observer in _observers)
                {
                    observer.OnNext(Value);
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableProperty(T initialValue = default, IEqualityComparer<T> comparer = null)
        {
            _value = initialValue;
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber(() => Unsubscribe(observer));
        }

        private void Unsubscribe(IObserver<T> observer) =>
            _observers.Remove(observer);

        private class Unsubscriber : IDisposable
        {
            private Action _unsubscribe;

            public Unsubscriber(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                _unsubscribe();
                _unsubscribe = null;
            }
        }
    }
}
