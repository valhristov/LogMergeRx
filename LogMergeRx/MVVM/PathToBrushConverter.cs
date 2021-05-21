using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using System.Windows.Media;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public class PathToBrushConverter : IValueConverter
    {
        private static int _counter;

        private static readonly ImmutableArray<Brush> _brushes = ImmutableArray.Create<Brush>(
            new SolidColorBrush(Colors.DarkBlue),
            //new SolidColorBrush(Colors.DarkCoral),
            new SolidColorBrush(Colors.DarkCyan),
            //new SolidColorBrush(Colors.DarkGoldenrodYellow),
            new SolidColorBrush(Colors.DarkGray),
            new SolidColorBrush(Colors.DarkGreen),
            //new SolidColorBrush(Colors.DarkPink),
            new SolidColorBrush(Colors.DarkSalmon),
            new SolidColorBrush(Colors.DarkSeaGreen),
            //new SolidColorBrush(Colors.DarkSkyBlue),
            new SolidColorBrush(Colors.DarkSlateGray),
            //new SolidColorBrush(Colors.DarkSteelBlue),
            new SolidColorBrush(Colors.DarkGoldenrod),
            new SolidColorBrush(Colors.DarkKhaki),
            new SolidColorBrush(Colors.DarkOliveGreen),
            new SolidColorBrush(Colors.DarkSeaGreen),
            new SolidColorBrush(Colors.DarkSlateBlue)
            );

        private static readonly Cache<FileId, Brush> _map = new(GetNextBrush);

        private static Brush GetNextBrush(FileId path) =>
            _brushes[Interlocked.Increment(ref _counter) % _brushes.Length];

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is FileId fileId
                ? _map.Get(fileId)
                : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
