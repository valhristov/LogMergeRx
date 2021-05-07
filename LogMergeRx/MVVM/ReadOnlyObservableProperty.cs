using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;

namespace LogMergeRx
{
    public class ReadOnlyObservableProperty<T> : IObservable<T>, INotifyPropertyChanged
    {
        private T _value;
        private readonly IEqualityComparer<T> _comparer;
        private readonly IObservable<T> _source;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyObservableProperty(IObservable<T> source, T initialValue = default, IEqualityComparer<T> comparer = null)
        {
            _value = initialValue;
            _comparer = comparer ?? EqualityComparer<T>.Default;

            _source = source.Where(x => !_comparer.Equals(_value, x));

            _source.Subscribe(x => _value = x);
        }

        public T Value
        {
            get => _value;
            private set
            {
                if (_comparer.Equals(_value, value))
                {
                    return;
                }
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public IDisposable Subscribe(IObserver<T> observer) =>
            _source.Subscribe(observer);
    }
}
