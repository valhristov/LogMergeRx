using System;
using System.Diagnostics;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("{Date}:{Level}:{Message}")]
    public class LogEntry : IEquatable<LogEntry>
    {
        private readonly Lazy<int> _cachedHash;

        public LogEntry(string FileName, string Date, string Level, string Source, string Message)
        {
            this.FileName = FileName;
            this.Date = Date;
            this.Level = Level;
            this.Source = Source;
            this.Message = Message;

            _cachedHash = new Lazy<int>(() =>
            {
                var h = 17;
                h = h * 31 + FileName.GetHashCode();
                h = h * 31 + Date.GetHashCode();
                h = h * 31 + Level.GetHashCode();
                h = h * 31 + Source.GetHashCode();
                h = h * 31 + Message.GetHashCode();
                return h;
            });
        }

        public string FileName { get; }
        public string Date { get; }
        public string Level { get; }
        public string Source { get; }
        public string Message { get; }

        public override bool Equals(object obj) =>
        obj is LogEntry logEntry && Equals(logEntry);

        public bool Equals(LogEntry other) =>
            ReferenceEquals(this, other) ||
            FileName == other.FileName &&
        Date == other.Date &&
            Level == other.Level &&
            Source == other.Source &&
        Message == other.Message;

        public override int GetHashCode() =>
            _cachedHash.Value;
    }
}
