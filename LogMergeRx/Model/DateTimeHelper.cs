using System;

namespace LogMergeRx.Model
{
    public static class DateTimeHelper
    {
        public static double FromDateToSeconds(DateTime dateTime) =>
            dateTime.Subtract(new DateTime(2020, 01, 01)).TotalSeconds;

        public static DateTime FromSecondsToDate(double seconds) =>
            new DateTime(2020, 01, 01).AddSeconds(seconds);
    }
}
