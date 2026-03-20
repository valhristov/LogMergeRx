using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using LogMergeRx.Model;
using Xunit;

namespace LogMergeRx;

public class CsvParser3_Tests
{
    [Fact]
    public void Can_Parse_Sample_Csv_With_Multiline_Messages()
    {
        // Arrange
        var samplePath = Path.Combine("Helpers", "sample.csv");
        using var stream = File.OpenRead(samplePath);
        var fileId = new FileId(1);

        // Act
        var entries = CsvParser3.Parse(stream, fileId);

        // Assert
        entries.Should().HaveCount(12, "sample.csv contains 12 log entries");

        // Verify first entry (simple single-line)
        var first = entries[0];
        first.Date.ToString("yyyy-MM-dd HH:mm:ss,fff").Should().Be("2026-03-15 00:03:52,076");
        first.Level.Should().Be(LogLevel.INFO);
        first.Source.Should().Be("odes.ExternalCodesManager");
        first.Message.Should().Contain("Liz 26419 asked to download 336 codes");

        // Verify second entry
        var second = entries[1];
        second.Date.ToString("yyyy-MM-dd HH:mm:ss,fff").Should().Be("2026-03-15 00:03:52,112");
        second.Level.Should().Be(LogLevel.ERROR);
        second.Source.Should().Be("r.Gate.LinkUpSetupManager");
        second.Message.Should().Contain("Could get Liz Autonomy Target");

        // Verify third entry (multi-line message with stack trace in Message field)
        var third = entries[2];
        third.Date.ToString("yyyy-MM-dd HH:mm:ss,fff").Should().Be("2026-03-15 00:03:52,489");
        third.Level.Should().Be(LogLevel.WARN);
        third.Source.Should().Be("Inexto.S3r.DbContextScope");
        third.Message.Should().Contain("DbContext.SaveChanges caused a System.Data.Entity.Infrastructure.DbUpdateException");
        third.Message.Should().Contain("at Inexto.S3r.Foundation.DbContextScope.Implementations.DbContextScope.SaveChanges");
        third.Message.Should().Contain("at System.Threading._IOCompletionCallback.PerformIOCompletionCallback");
        third.Message.Should().Contain("Involved entities:");
        third.Message.Split(new[] { "\r\n" }, System.StringSplitOptions.None).Length.Should().BeGreaterThan(20, 
            "message should contain multiple lines from the stack trace");

        // Verify fourth entry (very long multi-line message with nested exceptions)
        var fourth = entries[3];
        fourth.Date.ToString("yyyy-MM-dd HH:mm:ss,fff").Should().Be("2026-03-15 00:03:52,497");
        fourth.Level.Should().Be(LogLevel.ERROR);
        fourth.Source.Should().Be("eptionToFaultErrorHandler");
        fourth.Message.Should().Contain("System.Data.Entity.Infrastructure.DbUpdateException");
        fourth.Message.Should().Contain("FK_DeviceSetups_LineSetups");
        fourth.Message.Should().Contain("MDGate_PRD");
        fourth.Message.Should().Contain("--- End of inner exception stack trace ---");
        fourth.Message.Should().Contain("Exception rethrown at [0]:");
        fourth.Message.Split(new[] { "\r\n" }, System.StringSplitOptions.None).Length.Should().BeGreaterThan(50, 
            "message should contain many lines from the nested exception stack traces");

        // Verify remaining simple entries
        entries[4].Date.ToString("yyyy-MM-dd HH:mm:ss,fff").Should().Be("2026-03-15 00:03:53,696");
        entries[4].Level.Should().Be(LogLevel.INFO);
        entries[4].Message.Should().Contain("Liz 26415");

        entries[11].Date.ToString("yyyy-MM-dd HH:mm:ss,fff").Should().Be("2026-03-15 00:04:05,759");
        entries[11].Level.Should().Be(LogLevel.INFO);
        entries[11].Message.Should().Contain("Liz 26406");

        // Verify all entries have correct FileId
        entries.Should().AllSatisfy(e => e.FileId.Should().Be(fileId));
    }

    [Fact]
    public void ReadTest()
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream) { AutoFlush = true };

        LogHelper.AppendHeaders(stream);

        var entries = new[] { LogHelper.Create("1", fileId: 1), LogHelper.Create("2", fileId: 1) };
        LogHelper.Append(stream, entries);
        var parsed = CsvParser3.Parse(stream, new FileId(1));
        parsed.Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);

        entries = new[] { LogHelper.Create("3", fileId: 1) };
        LogHelper.Append(stream, entries);
        CsvParser3.Parse(stream, new FileId(1)).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);

        entries = new[] { LogHelper.Create("4", fileId: 1) };
        LogHelper.Append(stream, entries);
        CsvParser3.Parse(stream, new FileId(1)).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);

        entries = new[] { new LogEntry(new FileId(1), new DateTime(2026, 03, 10, 21, 35, 14), LogLevel.NOTICE, "test", "some message here", @"multiline
exception
stack trace") };
        LogHelper.Append(stream, entries);
        CsvParser3.Parse(stream, new FileId(1)).Should().Equal(entries, LogEntryEqualityComparer.Default.Equals);
    }

    [Fact]
    public void Read_Empty_File()
    {
        using var stream = new MemoryStream();

        // The following should not throw exceptions
        CsvParser3.Parse(stream, new FileId(1));
        CsvParser3.Parse(stream, new FileId(1));
        CsvParser3.Parse(stream, new FileId(1));
    }

    [Fact]
    public void Can_Parse_Message_With_Unescaped_Newlines()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        
        // Write header
        writer.WriteLine("Date;Thread;ThreadId;Level;Logger;Message;ExceptionString;");
        
        // Write entry with multi-line message (newlines are NOT escaped within quotes)
        writer.WriteLine("\"2026-03-15 00:00:00,000\";\"    \";\"1\";\"ERR\";\"TestSource\";\"Line 1");
        writer.WriteLine("Line 2");
        writer.WriteLine("Line 3\";\"\"");
        writer.Flush();
        
        stream.Position = 0;
        var fileId = new FileId(1);

        // Act
        var entries = CsvParser3.Parse(stream, fileId);

        // Assert
        entries.Should().HaveCount(1);
        entries[0].Message.Should().Be("Line 1\r\nLine 2\r\nLine 3");
        entries[0].Level.Should().Be(LogLevel.ERROR);
        entries[0].Source.Should().Be("TestSource");
    }

    [Fact]
    public void Handles_Missing_Stack_Trace_Field()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        
        // Write header
        writer.WriteLine("Date;Thread;ThreadId;Level;Logger;Message;");
        
        // Write entry without ExceptionString field
        writer.WriteLine("\"2026-03-15 00:00:00,000\";\"    \";\"1\";\"INF\";\"Source\";\"Message\"");
        writer.Flush();
        
        stream.Position = 0;
        var fileId = new FileId(1);

        // Act
        var entries = CsvParser3.Parse(stream, fileId);

        // Assert
        entries.Should().HaveCount(1);
        entries[0].Message.Should().Be("Message");
    }
}