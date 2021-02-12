using System;
using System.Diagnostics;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("{Date}:{Level}:{Message}")]
    public class LogEntry
    {
        public string FileName { get; }
        public DateTime Date { get; }
        public string Level { get; }
        public string Source { get; }
        public string Message { get; }

        public LogEntry(string fileName, DateTime date, string level, string source, string message)
        {
            FileName = fileName;
            Date = date;
            Level = level;
            Source = source;
            Message = message;
        }
    }
}
