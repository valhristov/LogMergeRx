using System;
using System.Diagnostics;

namespace LogMergeRx
{
    public class Logger
    {
        internal static Func<T, T> Log<T>(string format) =>
            param =>
            {
                Log(param, format);
                return param;
            };

        public static T Log<T>(T value, string format)
        {
            Debug.WriteLine(format, value);
            return value;
        }
    }
}
