using System;
using System.IO;
using System.Linq;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public static class LogHelper
    {
        private static int counter;

        public static LogEntry Create(string message, LogLevel level = LogLevel.ERROR, string date = null, string source = "source", string fileName = "") =>
            new LogEntry(fileName, date ?? counter++.ToString("00"), level, source, message);

        public static void Append(FilePath path, params LogEntry[] entries) =>
            File.AppendAllLines(path, entries.Select(ToCsv));

        public static void AppendHeaders(Stream stream)
        {
            using var writer = new StreamWriter(stream, leaveOpen: true);

            stream.Seek(0, SeekOrigin.End);

            writer.WriteLine($"\"Date\";\"\";\"Level\";\"Source\";\"Message\"");

            writer.Flush();
        }

        public static void Append(Stream stream, params LogEntry[] entries)
        {
            using var writer = new StreamWriter(stream, leaveOpen: true);

            stream.Seek(0, SeekOrigin.End);

            var position = stream.Position;

            Array.ForEach(entries, entry => writer.WriteLine(ToCsv(entry)));

            writer.Flush();

            stream.Position = position;
        }

        private static string ToCsv(LogEntry entry) =>
            $"\"{entry.Date}\";\"\";\"{entry.Level}\";\"{entry.Source}\";\"{entry.Message}\"";
    }
}
