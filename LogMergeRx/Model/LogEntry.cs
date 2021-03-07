using System.Diagnostics;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("{Date}:{Level}:{Message}")]
    public class LogEntry
    {
        public LogEntry(string fileName, string date, LogLevel level, string source, string message)
        {
            FileName = fileName;
            Date = date;
            Level = level;
            Source = source;
            Message = message;
        }

        public string FileName { get; }
        public string Date { get; }
        public LogLevel Level { get; }
        public string Source { get; }
        public string Message { get; }
    }
}
