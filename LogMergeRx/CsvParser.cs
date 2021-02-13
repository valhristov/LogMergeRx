using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LogMergeRx.Model;
using Microsoft.VisualBasic.FileIO;

namespace LogMergeRx
{
    public static class CsvParser
    {
        public static IEnumerable<LogEntry> ReadAll(Stream stream)
        {
            using (var parser = new TextFieldParser(stream, Encoding.UTF8, true, true))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");

                while (!parser.EndOfData)
                {
                    yield return CreateLogEntry(parser.ReadFields());
                }
            }

            static LogEntry CreateLogEntry(string[] fields) =>
                new LogEntry(
                    FileName: string.Empty, // TODO
                    Date: ParseDate(fields[0]),
                    Level: fields[2].Trim(),
                    Source: fields[3].Trim(),
                    Message: fields[4]
                );

            static DateTime ParseDate(string date) =>
                DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss,fff", null);
        }
    }
}
