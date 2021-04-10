using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LogMergeRx.Model;

namespace LogMergeRx
{
    public static class LogHelper
    {
        private static readonly DateTime start = new DateTime(2021, 03, 25, 15, 05, 05, 0);
        private static int counter;

        public static LogEntry Create(string message, LogLevel level = LogLevel.ERROR, string source = "source", int fileId = default) =>
            new LogEntry(new FileId(fileId), start.AddMilliseconds(counter++), level, source, message);

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
            $"\"{entry.Date:yyyy-MM-dd HH:mm:ss,fff}\";\"\";\"{entry.Level}\";\"{entry.Source}\";\"{entry.Message}\"";

        public static async Task Rename(AbsolutePath from, AbsolutePath to)
        {
            var retries = 0;
            while (retries++ < 5)
            {
                try
                {
                    File.Move(from, to);
                    break;
                }
                catch (IOException)
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
