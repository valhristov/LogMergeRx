using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using CommunityToolkit.HighPerformance.Buffers;
using LogMergeRx.Model;
using Sylvan.Data.Csv;

namespace LogMergeRx
{
    public static class CsvParser2
    {
        private static readonly ReadOnlyMemory<char> NewLine = new ReadOnlyMemory<char>(new[] { '\r', '\n' });

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
                        var rawLevel = StringPool.Shared.GetOrAdd(csv.GetFieldSpan(2));
                        var level = ParseLevel(rawLevel);
                        // a new column ThreadId was added on index 2 at some point. We use this hacky way
                        // to detect if it is present and offset the reset of the columns.
                        threadOffset = level == LogLevel.UNKNOWN ? 1 : 0;
                    }

                    var stackTraceSpan = csv.GetFieldSpan(5 + threadOffset.Value).Trim();
                    entry = LogEntry.Create(
                        fileId: fileId,
                        date: StringPool.Shared.GetOrAdd(csv.GetFieldSpan(0)),
                        level: ParseLevel(StringPool.Shared.GetOrAdd(csv.GetFieldSpan(2 + threadOffset.Value))),
                        source: StringPool.Shared.GetOrAdd(csv.GetFieldSpan(3 + threadOffset.Value)),
                        message: StringPool.Shared.GetOrAdd(
                            stackTraceSpan.Length > 0
                                ? string.Concat(csv.GetFieldSpan(4 + threadOffset.Value), NewLine.Span, stackTraceSpan)
                                : csv.GetFieldSpan(4 + threadOffset.Value))
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
                    "ERROR " or "ERR" => LogLevel.ERROR,
                    "WARN  " or "WRN" => LogLevel.WARN,
                    "INFO  " or "INF" => LogLevel.INFO,
                    "NOTICE" or "NOT" => LogLevel.NOTICE,
                    "DEBUG " or "DBG" => LogLevel.DEBUG,
                    _ => LogLevel.UNKNOWN
                };
        }
    }
}
