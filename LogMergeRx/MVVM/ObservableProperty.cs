using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Subjects;

namespace LogMergeRx
{
    public class ObservableProperty<T> : IObservable<T>, INotifyPropertyChanged
    {
        private T _value;
        private readonly IEqualityComparer<T> _comparer;
        private readonly Subject<T> _subject = new Subject<T>();
        private readonly T _initialValue;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableProperty(T initialValue = default, IEqualityComparer<T> comparer = null)
        {
            _value = _initialValue = initialValue;
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

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
                _subject.OnNext(value);
            }
        }

        public bool IsInitial =>
            _comparer.Equals(_value, _initialValue);

        public void Reset() =>
            Value = _initialValue;

        public IDisposable Subscribe(IObserver<T> observer) =>
            _subject.Subscribe(observer);
    }
}
