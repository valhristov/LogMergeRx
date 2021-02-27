using LogMergeRx.Model;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogMergeRx
{
    public class CsvReader
    {
        private long _lastOffset;

        public List<LogEntry> Read(Stream stream)
        {
            stream.Seek(_lastOffset, SeekOrigin.Begin);

            var result = ReadToEnd(stream).ToList();

            _lastOffset = stream.Position - Environment.NewLine.Length;

            return result;
        }

        private static IEnumerable<LogEntry> ReadToEnd(Stream stream)
        {
            using var parser = new TextFieldParser(stream, Encoding.UTF8, true, true);

            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(";");

            while (!parser.EndOfData)
            {
                yield return CreateLogEntry(parser.ReadFields());
            }

            static LogEntry CreateLogEntry(string[] fields) =>
                new LogEntry(
                    FileName: string.Empty, // TODO
                    Date: fields[0],
                    Level: fields[2].Trim(),
                    Source: fields[3].Trim(),
                    Message: fields[4]
                );
        }
    }
}
