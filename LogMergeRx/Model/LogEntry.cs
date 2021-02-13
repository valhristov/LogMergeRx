using System;
using System.Diagnostics;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("{Date}:{Level}:{Message}")]
    public record LogEntry(
        string FileName,
        DateTime Date,
        string Level,
        string Source,
        string Message);
}
