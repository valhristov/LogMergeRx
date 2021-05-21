using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace LogMergeRx
{
    public class FileIdMapper : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var fileId = values.OfType<FileId>().FirstOrDefault();
            var loadedFiles = values.OfType<IList<FileViewModel>>().FirstOrDefault();

            var path = loadedFiles.FirstOrDefault(f => f.FileId == fileId);

            return path.RelativePath.Value.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
