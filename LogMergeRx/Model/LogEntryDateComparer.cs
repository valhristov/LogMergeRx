using System;
using System.Collections;
using System.Collections.Generic;

namespace LogMergeRx.Model
{
    public sealed class LogEntryDateComparer : IComparer, IComparer<LogEntry>
    {
        public int Compare(object x, object y) =>
            x is LogEntry xentry &&
            y is LogEntry yentry
                ? Compare(xentry, yentry)
                : 0;

        public int Compare(LogEntry x, LogEntry y) =>
            StringComparer.OrdinalIgnoreCase.Compare(x.Date, y.Date);
    }
}
