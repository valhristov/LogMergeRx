using System.Diagnostics;

namespace LogMergeRx.Model
{
    [DebuggerDisplay("{Date}:{Level}:{Message}")]
    public class LogEntry
    {
        public LogEntry(string FileName, string Date, string Level, string Source, string Message)
        {
            this.FileName = FileName;
            this.Date = Date;
            this.Level = Level;
            this.Source = Source;
            this.Message = Message;
        }

        public string FileName { get; }
        public string Date { get; }
        public string Level { get; }
        public string Source { get; }
        public string Message { get; }
    }
}
