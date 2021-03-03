using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LogMergeRx.Model
{
    /// <summary>
    /// Equality comparer for testing purposes
    /// </summary>
    public sealed class LogEntryComparer : IEqualityComparer<LogEntry>
    {
        public static IEqualityComparer<LogEntry> Default { get; } =
            new LogEntryComparer();

        private LogEntryComparer() { }

        public bool Equals(LogEntry x, LogEntry y) =>
            ReferenceEquals(x, y) ||
            x.FileName == y.FileName &&
            x.Date == y.Date &&
            x.Level == y.Level &&
            x.Source == y.Source &&
            x.Message == y.Message;

        public int GetHashCode([DisallowNull] LogEntry obj)
        {
            var h = 17;
            h = h * 31 + obj.FileName.GetHashCode();
            h = h * 31 + obj.Date.GetHashCode();
            h = h * 31 + obj.Level.GetHashCode();
            h = h * 31 + obj.Source.GetHashCode();
            h = h * 31 + obj.Message.GetHashCode();
            return h;
        }
    }
}
