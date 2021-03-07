using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LogMergeRx;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRxTests
{
    [TestClass]
    public class LogMonitor_Tests
    {
        public TestContext TestContext { get; set; }

        private List<FilePath> Files { get; set; }
        private List<LogEntry> Entries { get; set; }
        private LogMonitor LogMonitor { get; set; }
        private string LogsPath { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            LogsPath = Path.Combine(TestContext.TestRunDirectory, "logs", TestContext.TestName);

            Directory.CreateDirectory(LogsPath);

            Files = new List<FilePath>();
            Entries = new List<LogEntry>();

            LogMonitor = new LogMonitor(LogsPath);
            LogMonitor.ChangedFiles.Subscribe(Files.Add);
            LogMonitor.ReadEntries.Subscribe(Entries.AddRange);
        }

        [TestMethod]
        public async Task Read_Modified_Files()
        {
            LogMonitor.Start();

            Write("log1.csv", LogHelper.Create("1"));
            Write("log1.csv", LogHelper.Create("2"));
            Write("log2.csv", LogHelper.Create("3"));
            Write("log2.csv", LogHelper.Create("4"));

            await Task.Delay(100);

            Files.Count.Should().Be(4);
            Files.Select(x => x.Name).Distinct().Should().Equal("log1.csv", "log2.csv");
            Entries.Count.Should().Be(4);
            Entries.Select(x => x.Message).Should().Equal("1", "2", "3", "4");
        }

        [TestMethod]
        public async Task Read_Existing_Files()
        {
            Write("log1.csv", LogHelper.Create("1"));
            Write("log1.csv", LogHelper.Create("2"));
            Write("log2.csv", LogHelper.Create("3"));
            Write("log2.csv", LogHelper.Create("4"));

            LogMonitor.Start();

            await Task.Delay(500);

            Files.Count.Should().Be(2); // We start after the file was last modified
            Files.Select(x => x.Name).Distinct().Should().Equal("log1.csv", "log2.csv");
            Entries.Count.Should().Be(4);
            Entries.Select(x => x.Message).Should().Equal("1", "2", "3", "4");
        }

        private void Write(string fileName, params LogEntry[] entries) =>
            LogHelper.Append((FilePath)Path.Combine(LogsPath, fileName), entries);
    }
}
