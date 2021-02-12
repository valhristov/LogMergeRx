using System.Collections.Generic;
using System.ComponentModel;

namespace LogMergeRx
{
    public class ObservableProperty<T> : ObservableBase<T>, INotifyPropertyChanged
    {
        private T _value;
        private readonly IEqualityComparer<T> _comparer;

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
                NotifyObservers(value);
            }
        }
    }
}
