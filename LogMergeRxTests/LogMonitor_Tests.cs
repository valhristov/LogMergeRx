using System;
using System.Collections.Generic;
using System.IO;
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
            LogsPath = Path.Combine(TestContext.TestRunDirectory, "logs");

            Directory.CreateDirectory(LogsPath);

            Files = new List<FilePath>();
            Entries = new List<LogEntry>();

            LogMonitor = new LogMonitor(LogsPath);
            LogMonitor.ChangedFiles.Subscribe(Files.Add);
            LogMonitor.ReadEntries.Subscribe(Entries.AddRange);
            LogMonitor.Start();
        }

        [TestMethod]
        public async Task MyTestMethod()
        {
            Write("log1.csv", LogHelper.Create("1"));
            Write("log1.csv", LogHelper.Create("2"));
            Write("log1.csv", LogHelper.Create("3"));
            Write("log1.csv", LogHelper.Create("4"));
            Write("log1.csv", LogHelper.Create("5"));

            await Task.Delay(100);

            Files.Count.Should().Be(5);
            Entries.Count.Should().Be(5);

            var entry2 = LogHelper.Create("5");

            Write("log1.csv", entry2);

            await Task.Delay(1000);

            Files.Count.Should().Be(6);
            Entries.Count.Should().Be(6);
        }

        private void Write(string fileName, params LogEntry[] entries) =>
            LogHelper.Append((FilePath)Path.Combine(LogsPath, fileName), entries);
    }
}
