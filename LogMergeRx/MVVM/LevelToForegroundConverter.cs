using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public class LevelToForegroundConverter : IValueConverter
    {
        public Brush Error { get; set; }
        public Brush Warning { get; set; }
        public Brush Notice { get; set; }
        public Brush Info { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is LogLevel level
                ? level switch
                    {
                        LogLevel.ERROR => Error,
                        LogLevel.WARN => Warning,
                        LogLevel.NOTICE => Notice,
                        LogLevel.INFO => Info,
                        _ => Info,
                    }
                : Info;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
