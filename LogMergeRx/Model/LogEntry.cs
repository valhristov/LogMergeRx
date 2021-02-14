using System.Diagnostics;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("{Date}:{Level}:{Message}")]
    public record LogEntry(
        string FileName,
        string Date,
        string Level,
        string Source,
        string Message);
}
