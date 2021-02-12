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

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableProperty(T initialValue = default, IEqualityComparer<T> comparer = null)
        {
            _value = initialValue;
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

        public IDisposable Subscribe(IObserver<T> observer) =>
            _subject.Subscribe(observer);
    }
}
