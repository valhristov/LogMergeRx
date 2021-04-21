using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LogMergeRx.Model
{
    /// <summary>
    /// Equality comparer for testing purposes
    /// </summary>
    public sealed class LogEntryEqualityComparer : IEqualityComparer<LogEntry>
    {
        public static IEqualityComparer<LogEntry> Default { get; } =
            new LogEntryEqualityComparer();

        private LogEntryEqualityComparer() { }

        public bool Equals(LogEntry x, LogEntry y) =>
            ReferenceEquals(x, y) ||
            x.FileId == y.FileId &&
            x.Date == y.Date &&
            x.Level == y.Level &&
            x.Source == y.Source &&
            x.Message == y.Message;

        public int GetHashCode([DisallowNull] LogEntry obj) =>
            HashCode.Combine(obj.FileId, obj.Date, obj.Level, obj.Source, obj.Message);
    }
}
