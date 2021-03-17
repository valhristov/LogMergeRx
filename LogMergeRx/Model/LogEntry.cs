using System.Diagnostics;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("{Date}:{Level}:{Message}")]
    public class LogEntry
    {
        public LogEntry(RelativePath path, string date, LogLevel level, string source, string message)
        {
            RelativePath = path.Value;
            Date = date;
            Level = level;
            Source = source;
            Message = message;
        }

        public string RelativePath { get; }
        public string Date { get; }
        public LogLevel Level { get; }
        public string Source { get; }
        public string Message { get; }
    }
}
