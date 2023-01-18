using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using LogMergeRx.Model;
using Sylvan.Data.Csv;

namespace LogMergeRx
{
    public static class CsvParserSylvan
    {
        public static ImmutableArray<LogEntry> Parse(Stream stream, FileId fileId) =>
            ReadToEnd(stream, fileId)
                .ToImmutableArray();

        private static IEnumerable<LogEntry> ReadToEnd(Stream stream, FileId fileId)
        {
            using var textReader = new StreamReader(stream, leaveOpen: true);
            using var csv = CsvDataReader.Create(textReader);

            if (stream.Position == 0 && csv.Read())
            {
                // TODO? csv.ReadHeader();
            }

            int? threadOffset = null;
            while (csv.Read())
            {
                var originalOffset = stream.Position;
                LogEntry entry = null;
                try
                {
                    if (threadOffset == null)
                    {
                        var level = ParseLevel(csv.GetString(2)?.Trim());
                        // a new column ThreadId was added on index 2 at some point. We use this hacky way
                        // to detect if it is present and offset the reset of the columns.
                        threadOffset = level == LogLevel.UNKNOWN ? 1 : 0;
                    }

                    entry = LogEntry.Create(
                        fileId: fileId,
                        date: csv.GetString(0),
                        level: ParseLevel(csv.GetString(2 + threadOffset.Value)?.Trim()),
                        source: csv.GetString(3 + threadOffset.Value)?.Trim(),
                        message: csv.GetString(4 + threadOffset.Value) // + (csv.GetString(5 + threadOffset.Value, out var exceptionMessage) && exceptionMessage.Length > 0 ? $"\r\n{exceptionMessage}" : string.Empty)
                        );
                }
                catch
                {
                    stream.Seek(originalOffset, SeekOrigin.Begin);
                    yield break;
                }
                yield return entry;
            }

            static LogLevel ParseLevel(string level) =>
                level.ToUpperInvariant() switch
                {
                    "ERROR" => LogLevel.ERROR,
                    "WARN" => LogLevel.WARN,
                    "INFO" => LogLevel.INFO,
                    "NOTICE" => LogLevel.NOTICE,
                    "DEBUG" => LogLevel.DEBUG,
                    _ => LogLevel.UNKNOWN
                };
        }
    }
}
