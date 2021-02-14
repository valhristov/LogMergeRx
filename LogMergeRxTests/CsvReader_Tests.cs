using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LogMergeRx
{
    [TestClass]
    public class CsvReader_Tests
    {
        [TestMethod]
        public void ReadTest()
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            var csv = new CsvReader();

            var entries = new[] { CreateLogEntry("1"), CreateLogEntry("2") };
            Append(stream, entries);
            csv.Read(stream).Should().Equal(entries);

            entries = new[] { CreateLogEntry("3") };
            Append(stream, entries);
            csv.Read(stream).Should().Equal(entries);

            entries = new[] { CreateLogEntry("4") };
            Append(stream, entries);
            csv.Read(stream).Should().Equal(entries);
        }

        private static LogEntry CreateLogEntry(string message) =>
            new LogEntry("", new DateTime(2021, 2, 20, 10, 10, 10, 100, DateTimeKind.Local), "error", "source", message);

        private static void Append(Stream stream, LogEntry[] entries)
        {
            using var writer =
                new StreamWriter(stream, leaveOpen: true)
                {
                    AutoFlush = true,
                };

            stream.Seek(0, SeekOrigin.End);

            Array.ForEach(entries, entry =>
                writer.WriteLine($"\"{entry.Date:yyyy-MM-dd HH:mm:ss,fff}\"; \"\"; \"{entry.Level}\"; \"{entry.Source}\"; \"{entry.Message}\""));
        }
    }
}