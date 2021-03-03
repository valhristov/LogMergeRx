using System;
using System.IO;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            var csv = new CsvReader("");

            var entries = new[] { CreateLogEntry("1"), CreateLogEntry("2") };
            Append(stream, entries);
            csv.Read(stream).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);

            entries = new[] { CreateLogEntry("3") };
            Append(stream, entries);
            csv.Read(stream).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);

            entries = new[] { CreateLogEntry("4") };
            Append(stream, entries);
            csv.Read(stream).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);
        }

        private static int counter;
        private static LogEntry CreateLogEntry(string message) =>
            new LogEntry("", counter++.ToString("00"), "error", "source", message);

        private static void Append(Stream stream, LogEntry[] entries)
        {
            using var writer =
                new StreamWriter(stream, leaveOpen: true)
                {
                    AutoFlush = true,
                };

            stream.Seek(0, SeekOrigin.End);

            Array.ForEach(entries, entry =>
                writer.WriteLine($"\"{entry.Date}\";\"\";\"{entry.Level}\";\"{entry.Source}\";\"{entry.Message}\";"));
        }
    }
}