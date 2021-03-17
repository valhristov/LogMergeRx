using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public static class LogHelper
    {
        private static int counter;

        public static LogEntry Create(string message, LogLevel level = LogLevel.ERROR, string date = null, string source = "source", string fileName = default) =>
            new LogEntry(RelativePath.FromPath(fileName), date ?? counter++.ToString("00"), level, source, message);

        public static void Append(AbsolutePath path, params LogEntry[] entries) =>
            File.AppendAllLines(path, entries.Select(ToCsv));

        public static void AppendHeaders(AbsolutePath path) =>
            File.AppendAllLines(path, Headers());

        private static IEnumerable<string> Headers()
        {
            yield return $"\"Date\";\"\";\"Level\";\"Source\";\"Message\"";
        }

        public static void AppendHeaders(Stream stream) =>
            Append(stream, Headers());

        public static void Append(Stream stream, params LogEntry[] entries) =>
            Append(stream, entries.Select(ToCsv));

        private static void Append(Stream stream, IEnumerable<string> entries)
        {
            using var writer = new StreamWriter(stream, leaveOpen: true);

            stream.Seek(0, SeekOrigin.End);

            var position = stream.Position;

            foreach (var entry in entries)
            {
                writer.WriteLine(entry);
            }

            writer.Flush();

            stream.Position = position;
        }

        private static string ToCsv(LogEntry entry) =>
            $"\"{entry.Date}\";\"\";\"{entry.Level}\";\"{entry.Source}\";\"{entry.Message}\"";
    }
}
