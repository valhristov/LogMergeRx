using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LogMergeRx
{
    public class BooleanToScrollBarVisibilityConverter : IValueConverter
    {
        public ScrollBarVisibility TrueValue { get; set; } = ScrollBarVisibility.Visible;
        public ScrollBarVisibility FalseValue { get; set; } = ScrollBarVisibility.Disabled;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool isVisible && isVisible
                ? TrueValue
                : FalseValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
