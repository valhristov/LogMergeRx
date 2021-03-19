using System;
using System.Diagnostics;
using System.Globalization;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("{Date}:{Level}:{Message}")]
    public class LogEntry
    {
        public LogEntry(RelativePath path, string date, LogLevel level, string source, string message)
        {
            RelativePath = path.Value;
            Date = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture);
            Level = level;
            Source = source;
            Message = message;
        }

        public string RelativePath { get; }
        public DateTime Date { get; }
        public LogLevel Level { get; }
        public string Source { get; }
        public string Message { get; }
    }
}
