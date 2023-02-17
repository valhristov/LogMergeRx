using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogMergeRx
{
    public class BooleanToTextWrappingConverter : IValueConverter
    {
        public TextWrapping TrueValue { get; set; } = TextWrapping.Wrap;
        public TextWrapping FalseValue { get; set; } = TextWrapping.NoWrap;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool isVisible && isVisible
                ? TrueValue
                : FalseValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
