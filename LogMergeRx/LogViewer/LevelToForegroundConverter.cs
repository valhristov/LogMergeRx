using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LogMergeRx.LogViewer
{
    public class LevelToForegroundConverter : IValueConverter
    {
        public Brush Error { get; set; }
        public Brush Warning { get; set; }
        public Brush Notice { get; set; }
        public Brush Info { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string level
                ? level switch
                    {
                        "ERROR" => Error,
                        "WARN" => Warning,
                        "NOTICE" => Notice,
                        "INFO" => Info,
                        _ => Info,
                    }
                : Info;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
