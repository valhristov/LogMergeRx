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

        private List<RelativePath> Files { get; set; }
        private List<LogEntry> Entries { get; set; }
        private LogMonitor LogMonitor { get; set; }
        private AbsolutePath LogsPath { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            LogsPath = AbsolutePath.FromFullPath(Path.Combine(TestContext.TestRunDirectory, "logs", TestContext.TestName));

            Directory.CreateDirectory(LogsPath);

            Files = new List<RelativePath>();
            Entries = new List<LogEntry>();

            LogMonitor = new LogMonitor(LogsPath);
            LogMonitor.ChangedFiles.Subscribe(Files.Add);
            LogMonitor.ReadEntries.Subscribe(Entries.AddRange);
        }

        [TestMethod]
        public async Task Read_Modified_Files()
        {
            LogHelper.AppendHeaders(GetPath("log1.csv"));
            LogHelper.AppendHeaders(GetPath("log2.csv"));

            LogMonitor.Start();

            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("1"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("2"));
            LogHelper.Append(GetPath("log2.csv"), LogHelper.Create("3"));
            LogHelper.Append(GetPath("log2.csv"), LogHelper.Create("4"));

            await Task.Delay(100);

            Files.Count.Should().Be(6); //2 headers and 4 entries
            Files.Select(x => x.Value).Distinct().Should().Equal("log1.csv", "log2.csv");
            Entries.Count.Should().Be(4);
            Entries.Select(x => x.Message).Should().Equal("1", "2", "3", "4");
        }

        [TestMethod]
        public async Task Read_Existing_Files()
        {
            LogHelper.AppendHeaders(GetPath("log1.csv"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("1"));
            LogHelper.Append(GetPath("log1.csv"), LogHelper.Create("2"));
            LogHelper.AppendHeaders(GetPath("log2.csv"));
            LogHelper.Append(GetPath("log2.csv"), LogHelper.Create("3"));
            LogHelper.Append(GetPath("log2.csv"), LogHelper.Create("4"));

            LogMonitor.Start();

            await Task.Delay(500);

            Files.Count.Should().Be(2); // We start after the file was last modified
            Files.Select(x => x.Value).Distinct().Should().Equal("log1.csv", "log2.csv");

            Entries.Count.Should().Be(4);
            Entries.Select(x => x.Message).Should().Equal("1", "2", "3", "4");
        }

        private AbsolutePath GetPath(string fileName) =>
            AbsolutePath.FromFullPath(Path.Combine(LogsPath, fileName));
    }
}
