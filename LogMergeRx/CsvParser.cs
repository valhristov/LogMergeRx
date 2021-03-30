using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
                //        BadDataFound = null,
                //    MissingFieldFound = null,
            };

        public static List<LogEntry> Parse(Stream stream, RelativePath path) =>
            ReadToEnd(stream, path)
                .ToList();

        private static IEnumerable<LogEntry> ReadToEnd(Stream stream, RelativePath path)
        {
            using var textReader = new StreamReader(stream, leaveOpen: true);
            using var csv = new CsvHelper.CsvReader(textReader, _configuration);

            if (stream.Position == 0 && csv.Read())
            {
                csv.ReadHeader();
            }

            while (csv.Read())
            {
                var entry = LogEntry.Create(
                    path: path,
                    date: csv.GetField<string>(0),
                    level: ParseLevel(csv.GetField<string>(2)?.Trim()),
                    source: csv.GetField<string>(3)?.Trim(),
                    message: csv.GetField<string>(4));

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
