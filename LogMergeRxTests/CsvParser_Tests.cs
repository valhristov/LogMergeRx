using System.IO;
using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{
    [TestClass]
    public class CsvParser_Tests
    {
        [TestMethod]
        public void ReadTest()
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            LogHelper.AppendHeaders(stream);

            var entries = new[] { LogHelper.Create("1", fileId: 1), LogHelper.Create("2", fileId: 1) };
            LogHelper.Append(stream, entries);
            CsvParser.Parse(stream, new FileId(1)).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);

            entries = new[] { LogHelper.Create("3", fileId: 1) };
            LogHelper.Append(stream, entries);
            CsvParser.Parse(stream, new FileId(1)).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);

            entries = new[] { LogHelper.Create("4", fileId: 1) };
            LogHelper.Append(stream, entries);
            CsvParser.Parse(stream, new FileId(1)).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);
        }

        [TestMethod]
        public void Read_Empty_File()
        {
            using var stream = new MemoryStream();

            // The following should not throw exceptions
            CsvParser.Parse(stream, new FileId(1));
            CsvParser.Parse(stream, new FileId(1));
            CsvParser.Parse(stream, new FileId(1));
        }
    }
}