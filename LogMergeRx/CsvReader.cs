using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper.Configuration;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public class CsvReader
    {
        private static readonly CsvConfiguration _configuration =
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };

        private long _lastOffset;
        private readonly string _fileName;

        public CsvReader(string fileName)
        {
            _fileName = fileName;
        }

        public List<LogEntry> Read(Stream stream)
        {
            stream.Seek(_lastOffset, SeekOrigin.Begin);

            var result = ReadToEnd(stream).ToList();

            _lastOffset = stream.Position - Environment.NewLine.Length;

            return result;
        }

        private IEnumerable<LogEntry> ReadToEnd(Stream stream)
        {
            using var textReader = new StreamReader(stream, leaveOpen: true);
            using var csv = new CsvHelper.CsvReader(textReader, _configuration);

            csv.Read();
            while (csv.Read())
            {
                var entry = new LogEntry(
                    FileName: _fileName,
                    Date: csv.GetField<string>(0),
                    Level: csv.GetField<string>(2).Trim(),
                    Source: csv.GetField<string>(3).Trim(),
                    Message: csv.GetField<string>(4));

                yield return entry;
            }
        }
    }
}
