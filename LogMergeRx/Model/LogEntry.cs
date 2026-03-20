using System;
using System.Diagnostics;
using System.Globalization;

namespace LogMergeRx.Model;

[DebuggerDisplay("{Date}:{Level}:{Message}")]
public class LogEntry
{
    public LogEntry(FileId fileId, DateTime date, LogLevel level, string source, string message, string exception)
    {
        FileId = fileId;
        Date = date;
        Level = level;
        Source = source;
        Message = message;
        Exception = exception;
    }

    public FileId FileId { get; }
    public DateTime Date { get; }
    public LogLevel Level { get; }
    public string Source { get; }
    public string Message { get; }
    public string Exception { get; }

    public static LogEntry Create(FileId fileId, string date, LogLevel level, string source, string message, string exception) =>
        new(fileId,
            DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture),
            level,
            source,
            message,
            exception);
}
