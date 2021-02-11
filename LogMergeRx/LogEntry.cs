using System;

namespace LogMergeRx
{
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
