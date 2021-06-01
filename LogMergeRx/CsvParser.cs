using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public static class CsvParser
    {
        private static readonly CsvConfiguration _configuration =
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                LeaveOpen = true,
                BadDataFound = null,
                //    MissingFieldFound = null,
            };

        public static ImmutableList<LogEntry> Parse(Stream stream, FileId fileId) =>
            ReadToEnd(stream, fileId)
                .ToImmutableList();

        private static IEnumerable<LogEntry> ReadToEnd(Stream stream, FileId fileId)
        {
            using var textReader = new StreamReader(stream, leaveOpen: true);
            using var csv = new CsvHelper.CsvReader(textReader, _configuration);

            if (stream.Position == 0 && csv.Read())
            {
                csv.ReadHeader();
            }

            while (csv.Read())
            {
                var originalOffset = stream.Position;
                LogEntry entry = null;
                try
                {
                    entry = LogEntry.Create(
                        fileId: fileId,
                        date: csv.GetField<string>(0),
                        level: ParseLevel(csv.GetField<string>(2)?.Trim()),
                        source: csv.GetField<string>(3)?.Trim(),
                        message: csv.GetField<string>(4) + (csv.TryGetField<string>(5, out var exceptionMessage) && exceptionMessage.Length > 0 ? $"\r\n{exceptionMessage}" : string.Empty));
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
                    _ => LogLevel.ERROR
                };
        }
    }
}
